using NeatShot.Common.Helpers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ImageEffectHelperTests
{
    [Fact]
    public void ApplyPixelate_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ImageEffectHelper.ApplyPixelate(null!, new Rect(0, 0, 10, 10)));
    }

    [Fact]
    public void ApplyBoxBlur_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ImageEffectHelper.ApplyBoxBlur(null!, new Rect(0, 0, 10, 10)));
    }

    [Fact]
    public void ApplyPixelate_ReturnsProcessedBitmap_WithUniformBlocks()
    {
        // Tạo ảnh 20x20: góc trái trên màu đỏ, góc phải trên màu xanh
        var width = 20;
        var height = 20;
        var stride = width * 4;
        var pixels = new byte[height * stride];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * stride + x * 4;
                if (x < 10)
                {
                    pixels[index + 2] = 200; // Red
                }
                else
                {
                    pixels[index] = 200; // Blue
                }
                pixels[index + 3] = 255; // Alpha
            }
        }

        var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        var result = ImageEffectHelper.ApplyPixelate(source, new Rect(0, 0, 20, 20), blockSize: 10);

        Assert.NotNull(result);
        Assert.Equal(20, result.PixelWidth);
        Assert.Equal(20, result.PixelHeight);

        var resultPixels = new byte[height * stride];
        result.CopyPixels(resultPixels, stride, 0);

        // Kiểm tra block đầu tiên (0,0) đến (9,9) có các pixel đồng nhất
        var firstPixelB = resultPixels[0];
        var firstPixelG = resultPixels[1];
        var firstPixelR = resultPixels[2];

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                int idx = y * stride + x * 4;
                Assert.Equal(firstPixelB, resultPixels[idx]);
                Assert.Equal(firstPixelG, resultPixels[idx + 1]);
                Assert.Equal(firstPixelR, resultPixels[idx + 2]);
            }
        }
    }

    [Fact]
    public void ApplyBoxBlur_SmoothsBoundaryPixels()
    {
        var width = 20;
        var height = 20;
        var stride = width * 4;
        var pixels = new byte[height * stride];

        // Nửa trái 0..9 là đen (0), nửa phải 10..19 là trắng (255)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * stride + x * 4;
                byte val = (byte)(x < 10 ? 0 : 255);
                pixels[index] = val;
                pixels[index + 1] = val;
                pixels[index + 2] = val;
                pixels[index + 3] = 255;
            }
        }

        var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        var result = ImageEffectHelper.ApplyBoxBlur(source, new Rect(0, 0, 20, 20), radius: 3);

        Assert.NotNull(result);
        var resultPixels = new byte[height * stride];
        result.CopyPixels(resultPixels, stride, 0);

        // Pixel tại ranh giới x=9 và x=10 phải được làm mờ (nằm giữa 0 và 255)
        int borderIdx1 = 10 * stride + 9 * 4;
        int borderIdx2 = 10 * stride + 10 * 4;

        Assert.True(resultPixels[borderIdx1] > 0 && resultPixels[borderIdx1] < 255);
        Assert.True(resultPixels[borderIdx2] > 0 && resultPixels[borderIdx2] < 255);
    }
}
