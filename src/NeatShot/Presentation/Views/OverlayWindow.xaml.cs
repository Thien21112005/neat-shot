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
    private DrawingToolType _previousTool = DrawingToolType.None;
    public OverlayViewModel ViewModel { get; }

    public OverlayWindow(OverlayViewModel viewModel, IExportService exportService)
    {
        InitializeComponent();

        ViewModel = viewModel;
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        DataContext = ViewModel;

        ViewModel.RequestClose += (s, e) => Close();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        SelectionControl.RegionMoved += (s, delta) => DrawingControl.OffsetElements(delta.X, delta.Y);

        DrawingControl.EyedropperHovered += OnDrawingEyedropperHovered;
        DrawingControl.EyedropperClicked += OnDrawingEyedropperClicked;

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
            }
        };
        Toolbar.ColorSelected += (s, color) => DrawingControl.CurrentColor = color;
        Toolbar.UndoRequested += (s, e) => DrawingControl.Undo();
        Toolbar.RedoRequested += (s, e) => DrawingControl.Redo();
        Toolbar.CloseRequested += (s, e) => ViewModel.CancelCommand.Execute(null);
        Toolbar.CopyRequested += async (s, e) => await ExecuteCopyAsync();
        Toolbar.SaveRequested += async (s, e) => await ExecuteSaveAsync();
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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayViewModel.SelectedRegion))
        {
            UpdateToolbarPosition(ViewModel.SelectedRegion);
        }
    }

    private void UpdateToolbarPosition(CaptureRegion region)
    {
        if (region.IsValid)
        {
            var rect = region.ToRect();
            Toolbar.Visibility = Visibility.Visible;

            // Đặt Toolbar nằm ngay phía dưới vùng chọn
            var toolbarLeft = rect.Right - 420; // căn phải theo vùng chọn
            if (toolbarLeft < rect.Left) toolbarLeft = rect.Left;
            if (toolbarLeft < 10) toolbarLeft = 10;

            var toolbarTop = rect.Bottom + 10;
            // Nếu sát đáy màn hình thì đưa Toolbar lên phía trên vùng chọn
            if (toolbarTop + 50 > ActualHeight)
            {
                toolbarTop = rect.Top - 45;
                if (toolbarTop < 10) toolbarTop = 10;
            }

            Canvas.SetLeft(Toolbar, toolbarLeft);
            Canvas.SetTop(Toolbar, toolbarTop);
        }
        else
        {
            Toolbar.Visibility = Visibility.Collapsed;
            Toolbar.ResetTools();
            DrawingControl.CurrentTool = DrawingToolType.None;
        }
    }

    public void Display(Rect virtualBounds, BitmapSource frozenScreen)
    {
        Left = virtualBounds.Left;
        Top = virtualBounds.Top;
        Width = virtualBounds.Width;
        Height = virtualBounds.Height;

        ViewModel.BackgroundImage = frozenScreen;

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
