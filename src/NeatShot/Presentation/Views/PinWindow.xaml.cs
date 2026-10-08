using NeatShot.Presentation.ViewModels;
using System.Windows;
using System.Windows.Input;
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
    }

    public void SetImage(BitmapSource image, Point initialPosition)
    {
        ArgumentNullException.ThrowIfNull(image);

        ViewModel.SetImage(image);
        Left = initialPosition.X;
        Top = initialPosition.Y;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        Close();
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
            Close();
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

    private void CopyImageToClipboard()
    {
        if (ViewModel.PinnedImage != null)
        {
            try
            {
                Clipboard.SetImage(ViewModel.PinnedImage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi sao chép ảnh ghim: {ex.Message}");
            }
        }
    }

    private void OnCopyImageClick(object sender, RoutedEventArgs e) => CopyImageToClipboard();
    private void OnResetZoomClick(object sender, RoutedEventArgs e) => ViewModel.ResetZoom();
    private void OnDecreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.DecreaseOpacity();
    private void OnIncreaseOpacityClick(object sender, RoutedEventArgs e) => ViewModel.IncreaseOpacity();
    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
