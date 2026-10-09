using NeatShot.Core.Services.Implementations;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class ScreenshotGalleryServiceTests : IDisposable
{
    private readonly string _testDir;

    public ScreenshotGalleryServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "NeatShot_GalleryTests_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try
            {
                Directory.Delete(_testDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static RenderTargetBitmap CreateDummyBitmap(int width = 120, int height = 80)
    {
        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Freeze();
        return rtb;
    }

    [Fact]
    public void GetScreenshotsDirectory_CreatesDirectoryIfMissing()
    {
        var service = new ScreenshotGalleryService(_testDir);

        Assert.False(Directory.Exists(_testDir));
        var dir = service.GetScreenshotsDirectory();

        Assert.Equal(_testDir, dir);
        Assert.True(Directory.Exists(_testDir));
    }

    [Fact]
    public async Task SaveScreenshotAsync_SavesValidPngFile()
    {
        var service = new ScreenshotGalleryService(_testDir);
        var bitmap = CreateDummyBitmap(200, 150);

        var savedPath = await service.SaveScreenshotAsync(bitmap, "test_shot.png");

        Assert.True(File.Exists(savedPath));
        Assert.Equal("test_shot.png", Path.GetFileName(savedPath));
        var fileInfo = new FileInfo(savedPath);
        Assert.True(fileInfo.Length > 0);
    }

    [Fact]
    public async Task GetSavedScreenshotsAsync_ReturnsListOrderedByDateDescending()
    {
        var service = new ScreenshotGalleryService(_testDir);
        var bitmap1 = CreateDummyBitmap(100, 50);
        var bitmap2 = CreateDummyBitmap(200, 100);

        var path1 = await service.SaveScreenshotAsync(bitmap1, "shot_1.png");
        File.SetLastWriteTime(path1, DateTime.Now.AddMinutes(-5));

        var path2 = await service.SaveScreenshotAsync(bitmap2, "shot_2.png");
        File.SetLastWriteTime(path2, DateTime.Now);

        var items = await service.GetSavedScreenshotsAsync();

        Assert.Equal(2, items.Count);
        Assert.Equal("shot_2.png", items[0].FileName);
        Assert.Equal("shot_1.png", items[1].FileName);
        Assert.Equal(200, items[0].PixelWidth);
        Assert.Equal(100, items[0].PixelHeight);
        Assert.Equal(100, items[1].PixelWidth);
        Assert.Equal(50, items[1].PixelHeight);
    }

    [Fact]
    public async Task DeleteScreenshot_DeletesFileSuccessfully()
    {
        var service = new ScreenshotGalleryService(_testDir);
        var bitmap = CreateDummyBitmap();
        var path = await service.SaveScreenshotAsync(bitmap, "delete_me.png");

        Assert.True(File.Exists(path));
        var deleted = service.DeleteScreenshot(path);

        Assert.True(deleted);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task LoadImage_LoadsBitmapSuccessfully()
    {
        var service = new ScreenshotGalleryService(_testDir);
        var bitmap = CreateDummyBitmap(160, 90);
        var path = await service.SaveScreenshotAsync(bitmap, "load_test.png");

        var loaded = service.LoadImage(path);

        Assert.NotNull(loaded);
        Assert.Equal(160, loaded.PixelWidth);
        Assert.Equal(90, loaded.PixelHeight);
    }
}
