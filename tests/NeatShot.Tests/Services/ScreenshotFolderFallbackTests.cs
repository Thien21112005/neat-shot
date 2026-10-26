using NeatShot.Common.Helpers;
using NeatShot.Core.Services.Implementations;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

/// <summary>
/// Windows Defender "Controlled folder access" có thể chặn app ghi vào Pictures/Documents.
/// Khi đó NeatShot phải lưu sang thư mục dự phòng thay vì im lặng thất bại.
/// </summary>
public class ScreenshotFolderFallbackTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "NeatShot_Fallback_" + Guid.NewGuid().ToString("N"));

    public ScreenshotFolderFallbackTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* bỏ qua */ }
    }

    /// <summary>Đường dẫn không thể tạo được (thư mục cha thực chất là một file).</summary>
    private string BlockedDirectory()
    {
        var file = Path.Combine(_root, "not-a-folder.txt");
        File.WriteAllText(file, "x");
        return Path.Combine(file, "NeatShot");
    }

    [Fact]
    public void TryPrepare_ReturnsTrue_AndCreatesAWritableFolder()
    {
        var dir = Path.Combine(_root, "Pictures", "NeatShot");

        Assert.True(WritableDirectory.TryPrepare(dir));
        Assert.True(Directory.Exists(dir));
        Assert.Empty(Directory.EnumerateFileSystemEntries(dir)); // file thử ghi đã được xoá
    }

    [Fact]
    public void TryPrepare_ReturnsFalse_WhenTheFolderCannotBeWritten()
    {
        Assert.False(WritableDirectory.TryPrepare(BlockedDirectory()));
    }

    [Fact]
    public void GetScreenshotsDirectory_FallsBack_WhenThePreferredFolderIsBlocked()
    {
        var fallback = Path.Combine(_root, "Fallback");
        var gallery = new ScreenshotGalleryService(BlockedDirectory(), fallbackDirectory: fallback);

        Assert.Equal(fallback, gallery.GetScreenshotsDirectory());
        Assert.True(gallery.IsUsingFallbackDirectory);
        Assert.True(Directory.Exists(fallback));
    }

    [Fact]
    public void GetScreenshotsDirectory_UsesThePreferredFolder_WhenItIsWritable()
    {
        var preferred = Path.Combine(_root, "Preferred");
        var gallery = new ScreenshotGalleryService(preferred, fallbackDirectory: Path.Combine(_root, "Fallback"));

        Assert.Equal(preferred, gallery.GetScreenshotsDirectory());
        Assert.False(gallery.IsUsingFallbackDirectory);
    }

    [Fact]
    public async Task SaveScreenshot_StillSucceeds_WhenThePreferredFolderIsBlocked()
    {
        var fallback = Path.Combine(_root, "Fallback");
        var gallery = new ScreenshotGalleryService(BlockedDirectory(), fallbackDirectory: fallback);
        var image = new WriteableBitmap(4, 4, 96, 96, PixelFormats.Bgra32, null);
        image.Freeze();

        var path = await gallery.SaveScreenshotAsync(image);

        Assert.StartsWith(fallback, path);
        Assert.True(File.Exists(path));
    }
}
