using NeatShot.Presentation.Views;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class GalleryPinPositionTests
{
    [Fact]
    public void CalculatePinPosition_UsesDipSize_ForHighDpiImages()
    {
        // Ảnh chụp ở 200% (192 DPI): 3000x1500 pixel = 1500x750 DIP
        var image = new RenderTargetBitmap(3000, 1500, 192, 192, PixelFormats.Pbgra32);

        var pos = GalleryWindow.CalculatePinPosition(image, new Size(1920, 1080));

        Assert.Equal(new Point(210, 165), pos);
    }

    [Fact]
    public void CalculatePinPosition_KeepsMinimumMargin_ForImagesLargerThanScreen()
    {
        var image = new RenderTargetBitmap(2500, 1400, 96, 96, PixelFormats.Pbgra32);

        var pos = GalleryWindow.CalculatePinPosition(image, new Size(1920, 1080));

        Assert.Equal(new Point(40, 40), pos);
    }
}
