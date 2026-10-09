using NeatShot.Presentation.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.Views;

/// <summary>
/// Cửa sổ thư viện ảnh chụp màn hình NeatShot.
/// </summary>
public partial class GalleryWindow : Window
{
    public GalleryViewModel ViewModel { get; }

    public GalleryWindow(GalleryViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        Loaded += async (s, e) => await ViewModel.LoadItemsAsync();
        ViewModel.PinRequested += OnPinRequested;
    }

    private void OnPinRequested(object? sender, BitmapSource image)
    {
        var pinWindow = new PinWindow(new PinViewModel());

        // Đặt vị trí ảnh ghim tại trung tâm màn hình chính
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        var x = Math.Max(40, (screenWidth - image.PixelWidth) / 2);
        var y = Math.Max(40, (screenHeight - image.PixelHeight) / 2);

        pinWindow.SetImage(image, new Point(x, y));
        pinWindow.Show();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.F5)
        {
            _ = ViewModel.LoadItemsAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
