using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.Views;

public partial class PinWindow : Window
{
    public PinViewModel ViewModel { get; }

    public PinWindow(PinViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        ViewModel.RequestClose += (s, e) => Close();

        AnnotationToolbar.ToolSelected += (s, tool) =>
        {
            if (tool == DrawingToolType.Select)
            {
                DrawingControl.CurrentTool = DrawingToolType.None;
            }
            else
            {
                DrawingControl.CurrentTool = tool;
            }
        };

        AnnotationToolbar.ColorSelected += (s, color) =>
        {
            DrawingControl.CurrentColor = color;
        };

        AnnotationToolbar.UndoRequested += (s, e) =>
        {
            DrawingControl.Undo();
        };

        AnnotationToolbar.CloseRequested += (s, e) =>
        {
            ToggleAllControls(false);
        };
    }

    public void SetImage(BitmapSource image, Point initialPosition)
    {
        ArgumentNullException.ThrowIfNull(image);

        ViewModel.SetImage(image);
        DrawingControl.BackgroundImage = image;
        Left = initialPosition.X;
        Top = initialPosition.Y;
    }

    /// <summary>
    /// Bật/tắt toàn bộ thanh điều khiển (header + toolbar vẽ) khi người dùng nhấp vào ảnh.
    /// </summary>
    public void ToggleAllControls(bool? show = null)
    {
        bool target;
        if (show.HasValue)
        {
            target = show.Value;
        }
        else
        {
            // Nếu có bất kỳ thanh nào đang hiện -> Tắt cả hai
            bool anyVisible = (PinControlBar.Visibility == Visibility.Visible || AnnotationToolbar.Visibility == Visibility.Visible);
            target = !anyVisible;
        }

        PinControlBar.Visibility = target ? Visibility.Visible : Visibility.Collapsed;
        AnnotationToolbar.Visibility = target ? Visibility.Visible : Visibility.Collapsed;

        if (!target)
        {
            DrawingControl.CurrentTool = DrawingToolType.None;
            AnnotationToolbar.SelectTool(DrawingToolType.None);
        }
        else
        {
            // Mặc định ở chế độ Select (Di chuyển/Chọn) để người dùng có thể nhấp lại vào ảnh để tắt
            DrawingControl.CurrentTool = DrawingToolType.None;
            AnnotationToolbar.SelectTool(DrawingToolType.Select);
        }
    }

    public void ToggleAnnotationToolbar(bool? show = null)
    {
        var target = show ?? (AnnotationToolbar.Visibility != Visibility.Visible);
        AnnotationToolbar.Visibility = target ? Visibility.Visible : Visibility.Collapsed;
        if (!target)
        {
            DrawingControl.CurrentTool = DrawingToolType.None;
            AnnotationToolbar.SelectTool(DrawingToolType.None);
        }
        else if (DrawingControl.CurrentTool == DrawingToolType.None)
        {
            AnnotationToolbar.SelectTool(DrawingToolType.Pencil);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
            return;

        // Bỏ qua nếu người dùng bấm trúng thanh điều khiển trên hoặc thanh công cụ vẽ dưới
        if (e.OriginalSource is DependencyObject dep)
        {
            if (IsVisualChildOf(dep, PinControlBar) || IsVisualChildOf(dep, AnnotationToolbar))
            {
                return;
            }
        }

        if (DrawingControl.CurrentTool == DrawingToolType.None)
        {
            var startLeft = Left;
            var startTop = Top;

            try
            {
                DragMove();
            }
            catch
            {
                // Bỏ qua lỗi nếu chuột đã nhả hoặc không thể kéo
            }

            var deltaX = Math.Abs(Left - startLeft);
            var deltaY = Math.Abs(Top - startTop);

            // Nếu cửa sổ bị dịch chuyển hơn 2px -> Đây là thao tác KÉO (Drag), KHÔNG thay đổi thanh công cụ
            if (deltaX > 2 || deltaY > 2)
            {
                return;
            }

            // Nếu cửa sổ KHÔNG dịch chuyển -> Đây là thao tác CLICK (Nhấn chuột vào ảnh)
            // Nhấn lần 1: xuất hiện thanh công cụ; Nhấn lần 2: tắt thanh công cụ
            ToggleAllControls();
        }
    }

    private static bool IsVisualChildOf(DependencyObject? child, DependencyObject? parent)
    {
        if (child == null || parent == null) return false;
        var current = child;
        while (current != null)
        {
            if (current == parent) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (e.Delta > 0)
            {
                ViewModel.IncreaseOpacity();
            }
            else
            {
                ViewModel.DecreaseOpacity();
            }
        }
        else
        {
            if (e.Delta > 0)
            {
                ViewModel.ZoomIn();
            }
            else
            {
                ViewModel.ZoomOut();
            }
        }

        e.Handled = true;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (ProcessShortcut(e.Key, Keyboard.Modifiers, e.OriginalSource))
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Xử lý phím tắt của cửa sổ ghim. Bỏ qua khi người dùng đang gõ chữ trong ô nhập văn bản chú thích.
    /// </summary>
    public bool ProcessShortcut(Key key, ModifierKeys modifiers, object? source = null)
    {
        if (source is TextBox)
        {
            return false;
        }

        if (key == Key.Escape)
        {
            if (PinControlBar.Visibility == Visibility.Visible || AnnotationToolbar.Visibility == Visibility.Visible)
            {
                ToggleAllControls(false);
                return true;
            }

            ViewModel.CloseCommand.Execute(null);
            return true;
        }

        if (key == Key.P && (modifiers == ModifierKeys.None || modifiers.HasFlag(ModifierKeys.Control)))
        {
            ToggleAnnotationToolbar();
            return true;
        }

        if (key == Key.C && modifiers.HasFlag(ModifierKeys.Control))
        {
            CopyImageToClipboard();
            return true;
        }

        if (key == Key.D0 && modifiers.HasFlag(ModifierKeys.Control))
        {
            ViewModel.ResetZoom();
            return true;
        }

        if (key == Key.OemPlus || key == Key.Add)
        {
            ViewModel.ZoomIn();
            return true;
        }

        if (key == Key.OemMinus || key == Key.Subtract)
        {
            ViewModel.ZoomOut();
            return true;
        }

        return false;
    }

    public BitmapSource GetAnnotatedImage()
    {
        if (ViewModel.PinnedImage == null) return null!;
        if (DrawingControl.UndoStack.Count == 0) return ViewModel.PinnedImage;

        var source = ViewModel.PinnedImage;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(source, new Rect(0, 0, source.Width, source.Height));

            // Lớp vẽ luôn ở toạ độ ảnh gốc (zoom là LayoutTransform), nên không cần quy đổi tỉ lệ
            foreach (var el in DrawingControl.UndoStack.Items)
            {
                DrawingRenderer.RenderElement(dc, el);
            }
        }

        var rtb = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
        rtb.Render(visual);
        rtb.Freeze();
        return rtb;
    }

    private void CopyImageToClipboard()
    {
        var imageToCopy = GetAnnotatedImage();
        if (imageToCopy != null)
        {
            try
            {
                Clipboard.SetImage(imageToCopy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi sao chép ảnh ghim: {ex.Message}");
            }
        }
    }

    private void OnAnnotateButtonClick(object sender, RoutedEventArgs e) => ToggleAnnotationToolbar();
    private void OnToggleAnnotateClick(object sender, RoutedEventArgs e) => ToggleAnnotationToolbar();
    private void OnCopyImageClick(object sender, RoutedEventArgs e) => CopyImageToClipboard();
    private void OnResetZoomClick(object sender, RoutedEventArgs e) => ViewModel.ResetZoom();
    private void OnDecreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.DecreaseOpacity();
    private void OnIncreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.IncreaseOpacity();
    private void OnCloseClick(object sender, RoutedEventArgs e) => ViewModel.CloseCommand.Execute(null);
}
