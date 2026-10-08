using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.ViewModels;

/// <summary>
/// ViewModel quản lý trạng thái cửa sổ ghim ảnh nổi (Always-on-Top).
/// </summary>
public partial class PinViewModel : ObservableObject
{
    public const double MinScale = 0.2;
    public const double MaxScale = 5.0;
    public const double ScaleStep = 0.1;

    public const double MinOpacity = 0.2;
    public const double MaxOpacity = 1.0;
    public const double OpacityStep = 0.1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScaledWidth))]
    private BitmapSource? _pinnedImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScaledWidth))]
    [NotifyPropertyChangedFor(nameof(ScaledHeight))]
    private double _scale = 1.0;

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScaledWidth))]
    private double _originalWidth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScaledHeight))]
    private double _originalHeight;

    public double ScaledWidth => Math.Max(1.0, OriginalWidth * Scale);
    public double ScaledHeight => Math.Max(1.0, OriginalHeight * Scale);

    public event EventHandler? RequestClose;

    public void SetImage(BitmapSource image)
    {
        ArgumentNullException.ThrowIfNull(image);

        PinnedImage = image;
        OriginalWidth = image.Width;
        OriginalHeight = image.Height;
        Scale = 1.0;
        Opacity = 1.0;
    }

    public void ZoomIn()
    {
        Scale = Math.Min(MaxScale, Math.Round(Scale + ScaleStep, 2));
    }

    public void ZoomOut()
    {
        Scale = Math.Max(MinScale, Math.Round(Scale - ScaleStep, 2));
    }

    public void ResetZoom()
    {
        Scale = 1.0;
    }

    public void IncreaseOpacity()
    {
        Opacity = Math.Min(MaxOpacity, Math.Round(Opacity + OpacityStep, 2));
    }

    public void DecreaseOpacity()
    {
        Opacity = Math.Max(MinOpacity, Math.Round(Opacity - OpacityStep, 2));
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
