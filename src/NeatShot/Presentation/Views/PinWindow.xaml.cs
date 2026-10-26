using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeatShot.Presentation.Views;

public partial class PinWindow : Window
{
    public PinViewModel ViewModel { get; }

    private readonly DispatcherTimer _hudTimer = new() { Interval = TimeSpan.FromSeconds(1.2) };

    public PinWindow(PinViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        ViewModel.RequestClose += (s, e) => Close();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (s, e) => _hudTimer.Stop();
        _hudTimer.Tick += (s, e) =>
        {
            _hudTimer.Stop();
            PinHud.Visibility = Visibility.Collapsed;
        };

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

        AnnotationToolbar.RedoRequested += (s, e) => DrawingControl.Redo();

        // Popup của thanh vẽ mở/đóng theo Visibility của thanh và luôn bám theo ảnh ghim khi kéo / phóng to
        AnnotationPopup.CustomPopupPlacementCallback = PlaceAnnotationToolbar;
        DependencyPropertyDescriptor.FromProperty(VisibilityProperty, typeof(UIElement))
            .AddValueChanged(AnnotationToolbar, (s, e) => SyncAnnotationPopup());
        IsVisibleChanged += (s, e) => SyncAnnotationPopup();
        LocationChanged += (s, e) => RepositionAnnotationPopup();
        SizeChanged += (s, e) => RepositionAnnotationPopup();
        Closed += (s, e) => AnnotationPopup.IsOpen = false;

        AnnotationToolbar.CloseRequested += (s, e) =>
        {
            ToggleAllControls(false);
        };
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PinViewModel.Scale) or nameof(PinViewModel.Opacity))
        {
            // Hiện nhãn trạng thái ngay, cập nhật chữ đồng bộ rồi tự ẩn sau một khoảng ngắn
            PinHudText.Text = ViewModel.HudText;
            PinHud.Visibility = Visibility.Visible;
            _hudTimer.Stop();
            _hudTimer.Start();
        }
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

        if (modifiers.HasFlag(ModifierKeys.Control) &&
            (key == Key.Y || (key == Key.Z && modifiers.HasFlag(ModifierKeys.Shift))))
        {
            DrawingControl.Redo();
            return true;
        }

        if (key == Key.Z && modifiers.HasFlag(ModifierKeys.Control))
        {
            DrawingControl.Undo();
            return true;
        }

        // Phím chữ chọn công cụ (giống màn hình chụp) — chỉ khi thanh vẽ đang mở
        if (modifiers == ModifierKeys.None && AnnotationToolbar.Visibility == Visibility.Visible &&
            ToolForKey(key) is { } tool)
        {
            AnnotationToolbar.SelectTool(tool);
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

    private static DrawingToolType? ToolForKey(Key key) => key switch
    {
        Key.V => DrawingToolType.Select,
        Key.H => DrawingToolType.Highlight,
        Key.R => DrawingToolType.Rectangle,
        Key.E => DrawingToolType.Ellipse,
        Key.L => DrawingToolType.Line,
        Key.A => DrawingToolType.Arrow,
        Key.T => DrawingToolType.Text,
        Key.N => DrawingToolType.StepCounter,
        Key.X => DrawingToolType.Pixelate,
        Key.B => DrawingToolType.Blur,
        _ => null
    };

    /// <summary>
    /// Mở/đóng Popup của thanh vẽ theo Visibility của thanh (chỉ mở khi cửa sổ ghim đang hiển thị).
    /// </summary>
    private void SyncAnnotationPopup()
    {
        AnnotationPopup.IsOpen = IsVisible && AnnotationToolbar.Visibility == Visibility.Visible;
    }

    /// <summary>
    /// WPF không tự dời Popup khi cửa sổ chủ di chuyển: đẩy nhẹ offset để buộc tính lại vị trí.
    /// </summary>
    private void RepositionAnnotationPopup()
    {
        if (!AnnotationPopup.IsOpen) return;
        var offset = AnnotationPopup.HorizontalOffset;
        AnnotationPopup.HorizontalOffset = offset + 0.01;
        AnnotationPopup.HorizontalOffset = offset;
    }

    /// <summary>
    /// Căn giữa thanh vẽ ngay dưới ảnh; nếu sát đáy màn hình thì đặt lên phía trên ảnh.
    /// </summary>
    private static CustomPopupPlacement[] PlaceAnnotationToolbar(Size popupSize, Size targetSize, Point offset)
    {
        var x = (targetSize.Width - popupSize.Width) / 2;
        return
        [
            new CustomPopupPlacement(new Point(x, targetSize.Height), PopupPrimaryAxis.Horizontal),
            new CustomPopupPlacement(new Point(x, -popupSize.Height), PopupPrimaryAxis.Horizontal)
        ];
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

    private void OnToggleAnnotateClick(object sender, RoutedEventArgs e) => ToggleAnnotationToolbar();
    private void OnCopyImageClick(object sender, RoutedEventArgs e) => CopyImageToClipboard();
    private void OnResetZoomClick(object sender, RoutedEventArgs e) => ViewModel.ResetZoom();
    private void OnDecreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.DecreaseOpacity();
    private void OnIncreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.IncreaseOpacity();
    private void OnCloseClick(object sender, RoutedEventArgs e) => ViewModel.CloseCommand.Execute(null);
}
