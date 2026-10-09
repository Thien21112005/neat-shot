using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.Converters;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class GalleryWindowTests
{
    private class DummyGalleryService : IScreenshotGalleryService
    {
        public bool DeleteScreenshot(string filePath) => true;
        public Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync() =>
            Task.FromResult<IReadOnlyList<GalleryItem>>(new List<GalleryItem>());
        public string GetScreenshotsDirectory() => @"C:\Pictures\NeatShot";
        public BitmapSource? LoadImage(string filePath) => null;
        public Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null) =>
            Task.FromResult(@"C:\Pictures\NeatShot\shot.png");
    }

    [Fact]
    public void GalleryWindow_CanBeInstantiated_AndAssignsViewModel_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var service = new DummyGalleryService();
                var vm = new GalleryViewModel(service);
                var window = new GalleryWindow(vm);

                Assert.NotNull(window);
                Assert.Same(vm, window.ViewModel);
                Assert.Same(vm, window.DataContext);
                Assert.Equal("Thư viện ảnh chụp NeatShot", window.Title);

                window.Close();
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException);
    }

    [Fact]
    public void FilePathToBitmapConverter_LoadsValidBitmap_WithoutLockingFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"neatshot_test_{Guid.NewGuid():N}.png");
        try
        {
            var rtb = new RenderTargetBitmap(80, 60, 96, 96, PixelFormats.Pbgra32);
            rtb.Freeze();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(stream);
            }

            var converter = new FilePathToBitmapConverter();
            var result = converter.Convert(tempFile, typeof(BitmapSource), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<BitmapSource>(result);

            // Xác nhận file không bị lock: có thể xóa ngay lập tức
            File.Delete(tempFile);
            Assert.False(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void FilePathToBitmapConverter_ReturnsNull_ForNonExistentFile()
    {
        var converter = new FilePathToBitmapConverter();
        var result = converter.Convert(@"C:\NonExistent_Path_12345.png", typeof(BitmapSource), null, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
