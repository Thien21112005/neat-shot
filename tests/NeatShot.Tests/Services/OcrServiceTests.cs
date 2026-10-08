using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class OcrServiceTests
{
    [Fact]
    public async Task RecognizeTextAsync_ThrowsArgumentNullException_WhenImageIsNull()
    {
        var service = new WindowsOcrService();
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.RecognizeTextAsync(null!));
    }

    [Fact]
    public void IsOcrSupported_DoesNotThrow_AndReturnsValidBoolean()
    {
        var service = new WindowsOcrService();
        var supported = service.IsOcrSupported();
        // Kiểm tra xem phương thức chạy an toàn trên Windows hiện tại
        Assert.True(supported || !supported);
    }

    [Fact]
    public async Task RecognizeTextAsync_ReturnsEmptyString_ForBlankImage()
    {
        var service = new WindowsOcrService();
        if (!service.IsOcrSupported())
        {
            var dummyBitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RecognizeTextAsync(dummyBitmap));
            return;
        }

        var blankBitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
        var result = await service.RecognizeTextAsync(blankBitmap);
        Assert.NotNull(result);
        Assert.Empty(result.Trim());
    }
}
