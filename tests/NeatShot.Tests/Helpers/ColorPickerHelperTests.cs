using NeatShot.Common.Helpers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ColorPickerHelperTests
{
    [Fact]
    public void GetPixelColor_ThrowsArgumentNullException_WhenBitmapIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ColorPickerHelper.GetPixelColor(null!, new Point(0, 0)));
    }

    [Fact]
    public void GetPixelColor_ExtractsAccuratePixelColor_FromBgra32Bitmap()
    {
        var bitmap = new WriteableBitmap(2, 2, 96, 96, PixelFormats.Bgra32, null);
        var pixels = new byte[]
        {
            0x00, 0x00, 0xFF, 0xFF, // (0,0): Red (B=0, G=0, R=255, A=255)
            0x00, 0xFF, 0x00, 0xFF, // (1,0): Green (B=0, G=255, R=0, A=255)
            0xFF, 0x00, 0x00, 0xFF, // (0,1): Blue (B=255, G=0, R=0, A=255)
            0xFF, 0xFF, 0xFF, 0xFF  // (1,1): White
        };
        bitmap.WritePixels(new Int32Rect(0, 0, 2, 2), pixels, 8, 0);

        var red = ColorPickerHelper.GetPixelColor(bitmap, new Point(0, 0), 96, 96);
        var green = ColorPickerHelper.GetPixelColor(bitmap, new Point(1, 0), 96, 96);
        var blue = ColorPickerHelper.GetPixelColor(bitmap, new Point(0, 1), 96, 96);
        var white = ColorPickerHelper.GetPixelColor(bitmap, new Point(1, 1), 96, 96);

        Assert.Equal(Color.FromRgb(255, 0, 0), red);
        Assert.Equal(Color.FromRgb(0, 255, 0), green);
        Assert.Equal(Color.FromRgb(0, 0, 255), blue);
        Assert.Equal(Color.FromRgb(255, 255, 255), white);
    }

    [Fact]
    public void GetPixelColor_ClampsCoordinates_WhenPointIsOutOfBounds()
    {
        var bitmap = new WriteableBitmap(2, 2, 96, 96, PixelFormats.Bgra32, null);
        var pixels = new byte[]
        {
            0x00, 0x00, 0xFF, 0xFF, // (0,0): Red
            0x00, 0xFF, 0x00, 0xFF, // (1,0): Green
            0xFF, 0x00, 0x00, 0xFF, // (0,1): Blue
            0x00, 0xFF, 0xFF, 0xFF  // (1,1): Yellow (B=0, G=255, R=255, A=255)
        };
        bitmap.WritePixels(new Int32Rect(0, 0, 2, 2), pixels, 8, 0);

        // Negative coordinates clamp to (0,0)
        var clampedTopLeft = ColorPickerHelper.GetPixelColor(bitmap, new Point(-10, -10), 96, 96);
        Assert.Equal(Color.FromRgb(255, 0, 0), clampedTopLeft);

        // Out of bounds bottom right clamps to (1,1)
        var clampedBottomRight = ColorPickerHelper.GetPixelColor(bitmap, new Point(100, 100), 96, 96);
        Assert.Equal(Color.FromRgb(255, 255, 0), clampedBottomRight);
    }

    [Fact]
    public void GetPixelColor_AccountsForDpiScaling_Correctly()
    {
        // 125% DPI scale: 96 * 1.25 = 120 DPI.
        // A DIP point (0.8, 0) with scale 1.25 maps to pixel px = round(0.8 * 1.25) = 1.
        var bitmap = new WriteableBitmap(2, 2, 120, 120, PixelFormats.Bgra32, null);
        var pixels = new byte[]
        {
            0x00, 0x00, 0xFF, 0xFF, // (0,0): Red
            0x00, 0xFF, 0x00, 0xFF, // (1,0): Green
            0xFF, 0x00, 0x00, 0xFF, // (0,1): Blue
            0xFF, 0xFF, 0xFF, 0xFF  // (1,1): White
        };
        bitmap.WritePixels(new Int32Rect(0, 0, 2, 2), pixels, 8, 0);

        // At DIP point (0.8, 0) on 120 DPI (125% scale), pixel X is 1 -> Green
        var color = ColorPickerHelper.GetPixelColor(bitmap, new Point(0.8, 0), 120, 120);
        Assert.Equal(Color.FromRgb(0, 255, 0), color);
    }
}
