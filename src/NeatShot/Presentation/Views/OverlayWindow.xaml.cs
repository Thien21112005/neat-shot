using Microsoft.Win32;
using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.Views;

public partial class OverlayWindow : Window
{
    private readonly IExportService _exportService;
    private readonly IOcrService _ocrService;
    private DrawingToolType _previousTool = DrawingToolType.None;
    private bool _isToolbarManuallyPositioned;
    private Point _manualToolbarPosition;
    public OverlayViewModel ViewModel { get; }

    public OverlayWindow(OverlayViewModel viewModel, IExportService exportService, IOcrService? ocrService = null)
    {
        InitializeComponent();

        ViewModel = viewModel;
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _ocrService = ocrService ?? new NeatShot.Core.Services.Implementations.WindowsOcrService();
        DataContext = ViewModel;

        ViewModel.RequestClose += (s, e) => Close();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        SelectionControl.RegionMoved += (s, delta) => DrawingControl.OffsetElements(delta.X, delta.Y);

        DrawingControl.EyedropperHovered += OnDrawingEyedropperHovered;
        DrawingControl.EyedropperClicked += OnDrawingEyedropperClicked;

        Toolbar.ToolbarMoved += OnToolbarMoved;
        Toolbar.ToolbarResetPosition += OnToolbarResetPosition;

        InitializeToolbar();
    }

    private void InitializeToolbar()
    {
        Toolbar.ToolSelected += (s, tool) =>
        {
            if (tool == DrawingToolType.Eyedropper)
            {
                EnterEyedropperMode();
            }
            else
            {
                ExitEyedropperMode();
                DrawingControl.CurrentTool = tool;
                if (tool == DrawingToolType.Text)
                {
                    DrawingControl.CurrentFontFamily = Toolbar.SelectedFontFamily;
                    DrawingControl.CurrentFontSize = Toolbar.SelectedFontSize;
                }
            }
        };
        Toolbar.FontFamilyChanged += (s, fontFamily) =>
        {
            DrawingControl.CurrentFontFamily = fontFamily;
            if (DrawingControl.SelectedElement != null && DrawingControl.SelectedElement.ToolType == DrawingToolType.Text)
            {
                DrawingControl.SelectedElement.FontFamily = fontFamily;
                DrawingControl.InvalidateVisual();
            }
        };
        Toolbar.FontSizeChanged += (s, fontSize) =>
        {
            DrawingControl.CurrentFontSize = fontSize;
            if (DrawingControl.SelectedElement != null && DrawingControl.SelectedElement.ToolType == DrawingToolType.Text)
            {
                DrawingControl.SelectedElement.FontSize = fontSize;
                DrawingControl.InvalidateVisual();
            }
        };
        Toolbar.UndoRequested += (s, e) => DrawingControl.Undo();
        Toolbar.RedoRequested += (s, e) => DrawingControl.Redo();
        Toolbar.CloseRequested += (s, e) => ViewModel.CancelCommand.Execute(null);
        Toolbar.CopyRequested += async (s, e) => await ExecuteCopyAsync();
        Toolbar.SaveRequested += async (s, e) => await ExecuteSaveAsync();
        Toolbar.PinRequested += async (s, e) => await ExecutePinAsync();
        Toolbar.OcrRequested += async (s, e) => await ExecuteOcrAsync();
    }

    private async Task ExecuteCopyAsync()
    {
        if (ViewModel.BackgroundImage == null || !ViewModel.SelectedRegion.IsValid)
            return;

        try
        {
            var finalImage = _exportService.RenderFinalImage(
                ViewModel.BackgroundImage,
                ViewModel.SelectedRegion,
                DrawingControl.UndoStack.Items);

            await _exportService.CopyToClipboardAsync(finalImage);
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi sao chép ảnh vào Clipboard: {ex.Message}");
        }
    }

    private async Task ExecuteSaveAsync()
    {
        if (ViewModel.BackgroundImage == null || !ViewModel.SelectedRegion.IsValid)
            return;

        try
        {
            var finalImage = _exportService.RenderFinalImage(
                ViewModel.BackgroundImage,
                ViewModel.SelectedRegion,
                DrawingControl.UndoStack.Items);

            var dialog = new SaveFileDialog
            {
                Title = "Lưu ảnh chụp màn hình NeatShot",
                Filter = "PNG Image (*.png)|*.png",
                DefaultExt = ".png",
                FileName = $"NeatShot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            if (dialog.ShowDialog(this) == true)
            {
                await _exportService.SaveToFileAsync(finalImage, dialog.FileName);
                Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi lưu file ảnh: {ex.Message}");
        }
    }

    private async Task ExecutePinAsync()
    {
        if (ViewModel.BackgroundImage == null || !ViewModel.SelectedRegion.IsValid)
            return;

        try
        {
            var finalImage = _exportService.RenderFinalImage(
                ViewModel.BackgroundImage,
                ViewModel.SelectedRegion,
                DrawingControl.UndoStack.Items);

            var pinWindow = new PinWindow(new PinViewModel());
            var screenX = Left + ViewModel.SelectedRegion.X;
            var screenY = Top + ViewModel.SelectedRegion.Y;
            pinWindow.SetImage(finalImage, new Point(screenX, screenY));
            pinWindow.Show();

            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi ghim ảnh: {ex.Message}");
        }
    }

    private async Task ExecuteOcrAsync()
    {
        if (ViewModel.BackgroundImage == null || !ViewModel.SelectedRegion.IsValid)
            return;

        try
        {
            var finalImage = _exportService.RenderFinalImage(
                ViewModel.BackgroundImage,
                ViewModel.SelectedRegion,
                DrawingControl.UndoStack.Items);

            var recognizedText = await _ocrService.RecognizeTextAsync(finalImage);

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                MessageBox.Show(
                    this,
                    "Không phát hiện thấy ký tự văn bản nào trong vùng chọn.",
                    "NeatShot - OCR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            CopyTextToClipboardWithRetry(recognizedText.Trim());
            Close();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "NeatShot - OCR",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi OCR: {ex.Message}");
            MessageBox.Show(
                this,
                $"Đã xảy ra lỗi trong quá trình nhận diện chữ: {ex.Message}",
                "NeatShot - OCR",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void CopyTextToClipboardWithRetry(string text)
    {
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Clipboard.SetText(text);
                return;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                if (attempt == maxAttempts) throw;
                Thread.Sleep(30);
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayViewModel.SelectedRegion))
        {
            UpdateToolbarPosition(ViewModel.SelectedRegion);
        }
        else if (e.PropertyName == nameof(OverlayViewModel.BackgroundImage))
        {
            DrawingControl.BackgroundImage = ViewModel.BackgroundImage;
        }
    }

    private void OnToolbarMoved(object? sender, Point delta)
    {
        var curX = Canvas.GetLeft(Toolbar);
        var curY = Canvas.GetTop(Toolbar);
        if (double.IsNaN(curX)) curX = 0;
        if (double.IsNaN(curY)) curY = 0;

        var newX = curX + delta.X;
        var newY = curY + delta.Y;

        var tbWidth = Toolbar.ActualWidth > 0 ? Toolbar.ActualWidth : 640;
        var tbHeight = Toolbar.ActualHeight > 0 ? Toolbar.ActualHeight : 44;

        _manualToolbarPosition = new Point(
            Math.Clamp(newX, 8, Math.Max(8, ActualWidth - tbWidth - 8)),
            Math.Clamp(newY, 4, Math.Max(4, ActualHeight - tbHeight - 4)));

        _isToolbarManuallyPositioned = true;
        Canvas.SetLeft(Toolbar, _manualToolbarPosition.X);
        Canvas.SetTop(Toolbar, _manualToolbarPosition.Y);
    }

    private void OnToolbarResetPosition(object? sender, EventArgs e)
    {
        _isToolbarManuallyPositioned = false;
        UpdateToolbarPosition(ViewModel.SelectedRegion);
    }

    private void OnWindowMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DrawingControl.CurrentTool != DrawingToolType.None)
        {
            Toolbar.ResetTools();
            DrawingControl.CurrentTool = DrawingToolType.None;
            e.Handled = true;
        }
    }

    private void UpdateToolbarPosition(CaptureRegion region)
    {
        if (region.IsValid)
        {
            var rect = region.ToRect();
            Toolbar.Visibility = Visibility.Visible;

            if (_isToolbarManuallyPositioned)
            {
                var tbWidth = Toolbar.ActualWidth > 0 ? Toolbar.ActualWidth : 640;
                var tbHeight = Toolbar.ActualHeight > 0 ? Toolbar.ActualHeight : 44;
                var clampedX = Math.Clamp(_manualToolbarPosition.X, 8, Math.Max(8, ActualWidth - tbWidth - 8));
                var clampedY = Math.Clamp(_manualToolbarPosition.Y, 4, Math.Max(4, ActualHeight - tbHeight - 4));
                Canvas.SetLeft(Toolbar, clampedX);
                Canvas.SetTop(Toolbar, clampedY);
                return;
            }

            var tbSize = new Size(
                Toolbar.ActualWidth > 0 ? Toolbar.ActualWidth : 640,
                Toolbar.ActualHeight > 0 ? Toolbar.ActualHeight : 44);
            var screenSize = new Size(
                ActualWidth > 0 ? ActualWidth : 1920,
                ActualHeight > 0 ? ActualHeight : 1080);

            var pos = ToolbarPositionHelper.CalculatePosition(rect, tbSize, screenSize);

            Canvas.SetLeft(Toolbar, pos.X);
            Canvas.SetTop(Toolbar, pos.Y);
        }
        else
        {
            Toolbar.Visibility = Visibility.Collapsed;
            Toolbar.ResetTools();
            DrawingControl.CurrentTool = DrawingToolType.None;
            _isToolbarManuallyPositioned = false;
        }
    }

    public void Display(Rect virtualBounds, BitmapSource frozenScreen)
    {
        Left = virtualBounds.Left;
        Top = virtualBounds.Top;
        Width = virtualBounds.Width;
        Height = virtualBounds.Height;

        ViewModel.BackgroundImage = frozenScreen;
        DrawingControl.BackgroundImage = frozenScreen;

        Show();
        Activate();
        Focus();
    }

    public void EnterEyedropperMode()
    {
        if (DrawingControl.CurrentTool != DrawingToolType.Eyedropper)
        {
            _previousTool = DrawingControl.CurrentTool;
        }
        DrawingControl.CurrentTool = DrawingToolType.Eyedropper;
        EyedropperLoupe.Visibility = Visibility.Visible;
    }

    public void ExitEyedropperMode()
    {
        EyedropperLoupe.Visibility = Visibility.Collapsed;
        if (DrawingControl.CurrentTool == DrawingToolType.Eyedropper)
        {
            DrawingControl.CurrentTool = _previousTool == DrawingToolType.Eyedropper
                ? DrawingToolType.None
                : _previousTool;
        }
    }

    private void OnDrawingEyedropperHovered(object? sender, Point pos)
    {
        if (ViewModel.BackgroundImage == null) return;

        var dpi = VisualTreeHelper.GetDpi(this);
        var color = ColorPickerHelper.GetPixelColor(
            ViewModel.BackgroundImage,
            pos,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY);

        LoupeColorSwatch.Background = new SolidColorBrush(color);
        LoupeHexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        var loupeLeft = pos.X + 16;
        var loupeTop = pos.Y + 16;

        if (loupeLeft + 140 > ActualWidth)
        {
            loupeLeft = pos.X - 150;
        }
        if (loupeTop + 60 > ActualHeight)
        {
            loupeTop = pos.Y - 60;
        }

        Canvas.SetLeft(EyedropperLoupe, Math.Max(0, loupeLeft));
        Canvas.SetTop(EyedropperLoupe, Math.Max(0, loupeTop));
        EyedropperLoupe.Visibility = Visibility.Visible;
    }

    private void OnDrawingEyedropperClicked(object? sender, Point pos)
    {
        if (ViewModel.BackgroundImage == null) return;

        var dpi = VisualTreeHelper.GetDpi(this);
        var color = ColorPickerHelper.GetPixelColor(
            ViewModel.BackgroundImage,
            pos,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY);

        DrawingControl.CurrentColor = color;
        Toolbar.AddColorToHistory(color);
        ExitEyedropperMode();
        Toolbar.SetActiveTool(_previousTool == DrawingToolType.None || _previousTool == DrawingToolType.Eyedropper ? DrawingToolType.Pencil : _previousTool);
    }

    /// <summary>
    /// Xử lý các phím tắt hệ thống và chuyển đổi công cụ vẽ (hỗ trợ kiểm thử trực tiếp).
    /// </summary>
    public bool ProcessShortcut(Key key, ModifierKeys modifiers, object? source = null)
    {
        if (source is TextBox)
        {
            return false;
        }

        if (key == Key.Escape)
        {
            if (DrawingControl.CurrentTool == DrawingToolType.Eyedropper)
            {
                ExitEyedropperMode();
                return true;
            }

            if (DrawingControl.CurrentTool != DrawingToolType.None)
            {
                Toolbar.ResetTools();
                DrawingControl.CurrentTool = DrawingToolType.None;
                return true;
            }

            ViewModel.CancelCommand.Execute(null);
            return true;
        }

        if (key == Key.Delete)
        {
            if (DrawingControl.SelectedElement != null)
            {
                DrawingControl.DeleteSelectedElement();
                return true;
            }
        }

        if (key == Key.Z && (modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            DrawingControl.Redo();
            return true;
        }

        if (key == Key.Y && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            DrawingControl.Redo();
            return true;
        }

        if (key == Key.Z && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            DrawingControl.Undo();
            return true;
        }

        if (key == Key.Enter || (key == Key.C && (modifiers & ModifierKeys.Control) == ModifierKeys.Control))
        {
            _ = ExecuteCopyAsync();
            return true;
        }

        if (key == Key.S && (modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            _ = ExecuteSaveAsync();
            return true;
        }

        if (modifiers == ModifierKeys.None)
        {
            switch (key)
            {
                case Key.V:
                    Toolbar.SetActiveTool(DrawingToolType.Select);
                    DrawingControl.CurrentTool = DrawingToolType.Select;
                    return true;
                case Key.P:
                    Toolbar.SetActiveTool(DrawingToolType.Pencil);
                    DrawingControl.CurrentTool = DrawingToolType.Pencil;
                    return true;
                case Key.R:
                    Toolbar.SetActiveTool(DrawingToolType.Rectangle);
                    DrawingControl.CurrentTool = DrawingToolType.Rectangle;
                    return true;
                case Key.O:
                    Toolbar.SetActiveTool(DrawingToolType.Ellipse);
                    DrawingControl.CurrentTool = DrawingToolType.Ellipse;
                    return true;
                case Key.L:
                    Toolbar.SetActiveTool(DrawingToolType.Line);
                    DrawingControl.CurrentTool = DrawingToolType.Line;
                    return true;
                case Key.A:
                    Toolbar.SetActiveTool(DrawingToolType.Arrow);
                    DrawingControl.CurrentTool = DrawingToolType.Arrow;
                    return true;
                case Key.H:
                    Toolbar.SetActiveTool(DrawingToolType.Highlight);
                    DrawingControl.CurrentTool = DrawingToolType.Highlight;
                    return true;
                case Key.T:
                    Toolbar.SetActiveTool(DrawingToolType.Text);
                    DrawingControl.CurrentTool = DrawingToolType.Text;
                    return true;
                case Key.B:
                    Toolbar.SetActiveTool(DrawingToolType.Pixelate);
                    DrawingControl.CurrentTool = DrawingToolType.Pixelate;
                    return true;
                case Key.N:
                    Toolbar.SetActiveTool(DrawingToolType.StepCounter);
                    DrawingControl.CurrentTool = DrawingToolType.StepCounter;
                    return true;
                case Key.I:
                    Toolbar.SetActiveTool(DrawingToolType.Eyedropper);
                    EnterEyedropperMode();
                    return true;
            }
        }

        return false;
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (ProcessShortcut(e.Key, Keyboard.Modifiers, e.OriginalSource))
        {
            e.Handled = true;
        }
    }
}
