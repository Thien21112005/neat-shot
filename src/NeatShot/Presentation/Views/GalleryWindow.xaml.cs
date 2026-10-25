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
        var screenSize = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        pinWindow.SetImage(image, CalculatePinPosition(image, screenSize));
        pinWindow.Show();
    }

    /// <summary>
    /// Tính vị trí căn giữa ảnh ghim theo đơn vị DIP (WPF 96 DPI), giữ lề tối thiểu 40px.
    /// </summary>
    public static Point CalculatePinPosition(BitmapSource image, Size screenSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        // image.Width/Height là kích thước DIP; PixelWidth chỉ đúng khi ảnh ở 96 DPI
        var x = Math.Max(40, (screenSize.Width - image.Width) / 2);
        var y = Math.Max(40, (screenSize.Height - image.Height) / 2);
        return new Point(x, y);
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
