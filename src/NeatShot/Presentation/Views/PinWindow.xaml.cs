using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using System.Windows;
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
            DrawingControl.CurrentTool = tool;
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
            ToggleAnnotationToolbar(false);
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
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            if (DrawingControl.CurrentTool == DrawingToolType.None)
            {
                if (AnnotationToolbar.Visibility != Visibility.Visible)
                {
                    ToggleAnnotationToolbar(true);
                }

                try
                {
                    DragMove();
                }
                catch
                {
                    // Bỏ qua lỗi nếu chuột đã nhả hoặc không thể kéo
                }
            }
        }
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
        if (e.Key == Key.Escape)
        {
            if (AnnotationToolbar.Visibility == Visibility.Visible && DrawingControl.CurrentTool != DrawingToolType.None)
            {
                ToggleAnnotationToolbar(false);
                e.Handled = true;
                return;
            }

            ViewModel.CloseCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.P && (Keyboard.Modifiers == ModifierKeys.None || Keyboard.Modifiers.HasFlag(ModifierKeys.Control)))
        {
            ToggleAnnotationToolbar();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            CopyImageToClipboard();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.D0 && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            ViewModel.ResetZoom();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.OemPlus || e.Key == Key.Add)
        {
            ViewModel.ZoomIn();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
        {
            ViewModel.ZoomOut();
            e.Handled = true;
            return;
        }
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
            var scaleX = source.Width / Math.Max(1.0, DrawingControl.ActualWidth);
            var scaleY = source.Height / Math.Max(1.0, DrawingControl.ActualHeight);
            if (DrawingControl.ActualWidth > 0 && (Math.Abs(scaleX - 1.0) > 0.001 || Math.Abs(scaleY - 1.0) > 0.001))
            {
                dc.PushTransform(new ScaleTransform(scaleX, scaleY));
                foreach (var el in DrawingControl.UndoStack.Items)
                {
                    DrawingRenderer.RenderElement(dc, el);
                }
                dc.Pop();
            }
            else
            {
                foreach (var el in DrawingControl.UndoStack.Items)
                {
                    DrawingRenderer.RenderElement(dc, el);
                }
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
