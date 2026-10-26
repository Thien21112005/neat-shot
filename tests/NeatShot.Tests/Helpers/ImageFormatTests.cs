using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ImageFormatTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "NeatShot_FormatTests_" + Guid.NewGuid().ToString("N"));

    public ImageFormatTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* bỏ qua lỗi dọn dẹp */ }
    }

    private static RenderTargetBitmap Bitmap()
    {
        var rtb = new RenderTargetBitmap(64, 48, 96, 96, PixelFormats.Pbgra32);
        rtb.Freeze();
        return rtb;
    }

    private static bool IsJpeg(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return bytes.Length > 2 && bytes[0] == 0xFF && bytes[1] == 0xD8;
    }

    private static bool IsPng(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == (byte)'P' && bytes[2] == (byte)'N' && bytes[3] == (byte)'G';
    }

    [Theory]
    [InlineData("shot.jpg", typeof(JpegBitmapEncoder))]
    [InlineData("shot.JPEG", typeof(JpegBitmapEncoder))]
    [InlineData("shot.png", typeof(PngBitmapEncoder))]
    [InlineData("shot", typeof(PngBitmapEncoder))]
    public void CreateEncoder_PicksTheEncoderFromTheFileExtension(string fileName, Type expected)
    {
        Assert.IsType(expected, ImageEncoderFactory.CreateForPath(fileName, jpegQuality: 85));
    }

    [Fact]
    public void CreateEncoder_AppliesJpegQuality()
    {
        var encoder = Assert.IsType<JpegBitmapEncoder>(ImageEncoderFactory.CreateForPath("a.jpg", jpegQuality: 70));
        Assert.Equal(70, encoder.QualityLevel);
    }

    [Theory]
    [InlineData("jpg", ".jpg")]
    [InlineData("JPG", ".jpg")]
    [InlineData("png", ".png")]
    [InlineData(null, ".png")]
    [InlineData("bmp", ".png")]
    public void ExtensionFor_NormalizesTheSettingsValue(string? format, string expected)
    {
        Assert.Equal(expected, ImageEncoderFactory.ExtensionFor(format));
    }

    [Fact]
    public async Task ExportService_SaveToFileAsync_WritesJpeg_ForJpgPaths()
    {
        var path = Path.Combine(_dir, "export.jpg");

        await new ExportService(new ScreenCaptureService()).SaveToFileAsync(Bitmap(), path);

        Assert.True(IsJpeg(path));
    }

    [Fact]
    public async Task GallerySave_UsesTheFormatFromSettings()
    {
        var settings = new SettingsService(Path.Combine(_dir, "settings.json"));
        settings.SaveSettings(new AppSettings { ImageFormat = "jpg", DefaultSaveDirectory = _dir });
        var gallery = new ScreenshotGalleryService(_dir, settings);

        var saved = await gallery.SaveScreenshotAsync(Bitmap());

        Assert.EndsWith(".jpg", saved);
        Assert.True(IsJpeg(saved));
    }

    [Fact]
    public async Task GallerySave_DefaultsToPng()
    {
        var gallery = new ScreenshotGalleryService(_dir);

        var saved = await gallery.SaveScreenshotAsync(Bitmap());

        Assert.EndsWith(".png", saved);
        Assert.True(IsPng(saved));
    }
}
