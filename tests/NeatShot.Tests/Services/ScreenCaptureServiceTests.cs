using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class ScreenCaptureServiceTests
{
    private readonly IScreenCaptureService _service = new ScreenCaptureService();

    [Fact]
    public void GetVirtualScreenBounds_ShouldReturnValidRect()
    {
        // Act
        var bounds = _service.GetVirtualScreenBounds();

        // Assert: Virtual screen phải có kích thước lớn hơn 0
        Assert.True(bounds.Width > 0);
        Assert.True(bounds.Height > 0);
    }

    [Fact]
    public void Crop_WithValidRegion_ShouldReturnCroppedBitmapSource()
    {
        // Arrange: Tạo một bitmap 400x300 trong memory
        var original = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
        var region = new CaptureRegion(50, 50, 200, 100);

        // Act
        var cropped = _service.Crop(original, region);

        // Assert: Kích thước pixel sau khi crop phải chính xác
        Assert.NotNull(cropped);
        Assert.Equal(200, cropped.PixelWidth);
        Assert.Equal(100, cropped.PixelHeight);
    }

    [Fact]
    public void Crop_WithInvertedRegion_ShouldNormalizeAndCropCorrectly()
    {
        // Arrange: Tạo bitmap và vùng chọn kéo ngược (Width, Height âm)
        var original = new RenderTargetBitmap(500, 400, 96, 96, PixelFormats.Pbgra32);
        var invertedRegion = new CaptureRegion(250, 150, -200, -100);

        // Act
        var cropped = _service.Crop(original, invertedRegion);

        // Assert
        Assert.NotNull(cropped);
        Assert.Equal(200, cropped.PixelWidth);
        Assert.Equal(100, cropped.PixelHeight);
    }

    [Fact]
    public void Crop_ClampedToBounds_WhenRegionExceedsOriginalBitmap_ShouldNotThrow()
    {
        // Arrange: Vùng crop vượt quá kích thước bitmap gốc
        var original = new RenderTargetBitmap(200, 200, 96, 96, PixelFormats.Pbgra32);
        var oversizedRegion = new CaptureRegion(150, 150, 200, 200);

        // Act
        var cropped = _service.Crop(original, oversizedRegion);

        // Assert: Tự động cắt gọn (clamp) trong phạm vi bitmap gốc (còn 50x50)
        Assert.NotNull(cropped);
        Assert.Equal(50, cropped.PixelWidth);
        Assert.Equal(50, cropped.PixelHeight);
    }

    [Fact]
    public async Task CaptureCleanScreenAsync_ReturnsValidBitmap()
    {
        // Act
        var bitmap = await _service.CaptureCleanScreenAsync();

        // Assert
        Assert.NotNull(bitmap);
        Assert.True(bitmap.PixelWidth > 0);
        Assert.True(bitmap.PixelHeight > 0);
        Assert.True(bitmap.DpiX >= 96.0);
        Assert.True(bitmap.DpiY >= 96.0);
    }

    [Fact]
    public void Crop_WithHighDpiBitmap_ShouldScaleCropCoordinatesCorrectly()
    {
        // 120 DPI (125% scale): DIP size 400x300, pixel size 500x375
        var highDpiSource = new RenderTargetBitmap(500, 375, 120, 120, PixelFormats.Pbgra32);
        var region = new CaptureRegion(100, 100, 200, 100);

        var cropped = _service.Crop(highDpiSource, region);

        Assert.NotNull(cropped);
        // In physical pixels: 200 * 1.25 = 250, 100 * 1.25 = 125
        Assert.Equal(250, cropped.PixelWidth);
        Assert.Equal(125, cropped.PixelHeight);
    }

    [Fact]
    public async Task CaptureCleanScreenAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await _service.CaptureCleanScreenAsync(cts.Token);
        });
    }
}
