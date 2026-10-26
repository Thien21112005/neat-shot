using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.IO;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowGalleryAutoSaveTests
{
    private sealed class FakeExportService : IExportService
    {
        public int CopyCount { get; private set; }

        public RenderTargetBitmap RenderFinalImage(BitmapSource background, CaptureRegion region, IEnumerable<DrawingElement> annotations, BeautifyOptions? beautifyOptions = null)
        {
            var rtb = new RenderTargetBitmap(10, 10, 96, 96, PixelFormats.Pbgra32);
            rtb.Freeze();
            return rtb;
        }

        public Task CopyToClipboardAsync(BitmapSource image)
        {
            CopyCount++;
            return Task.CompletedTask;
        }

        public Task SaveToFileAsync(BitmapSource image, string filePath) => Task.CompletedTask;
    }

    private sealed class FakeGalleryService : IScreenshotGalleryService
    {
        public List<BitmapSource> Saved { get; } = new();
        public bool ThrowOnSave { get; set; }

        public string GetScreenshotsDirectory() => "C:\\fake";
        public Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync() => Task.FromResult<IReadOnlyList<GalleryItem>>(Array.Empty<GalleryItem>());

        public Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null)
        {
            if (ThrowOnSave) throw new IOException("disk full");
            Saved.Add(image);
            return Task.FromResult("C:\\fake\\shot.png");
        }

        public bool DeleteScreenshot(string filePath) => false;
        public BitmapSource? LoadImage(string filePath) => null;
    }

    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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
    public void OverlayWindow_Copy_AlsoSavesCaptureToGallery_OnStaThread()
    {
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var export = new FakeExportService();
            var gallery = new FakeGalleryService();
            var window = new OverlayWindow(vm, export, galleryService: gallery);

            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);

            Assert.Equal(1, export.CopyCount);
            Assert.Single(gallery.Saved);
        });
    }

    [Fact]
    public void OverlayWindow_Copy_RaisesCaptureCopied_WithSavedPath_OnStaThread()
    {
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var window = new OverlayWindow(vm, new FakeExportService(), galleryService: new FakeGalleryService());
            string? copiedPath = "not raised";
            window.CaptureCopied += (s, path) => copiedPath = path;

            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);

            Assert.Equal(@"C:\fake\shot.png", copiedPath);
        });
    }

    [Fact]
    public void OverlayWindow_Copy_RaisesCaptureCopied_WithNullPath_WhenGallerySaveFails_OnStaThread()
    {
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var window = new OverlayWindow(vm, new FakeExportService(), galleryService: new FakeGalleryService { ThrowOnSave = true });
            string? copiedPath = "not raised";
            window.CaptureCopied += (s, path) => copiedPath = path;

            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);

            Assert.Null(copiedPath);
        });
    }

    [Fact]
    public void OverlayWindow_Copy_StillCopies_WhenGallerySaveFails_OnStaThread()
    {
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var export = new FakeExportService();
            var gallery = new FakeGalleryService { ThrowOnSave = true };
            var window = new OverlayWindow(vm, export, galleryService: gallery);

            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);

            Assert.Equal(1, export.CopyCount);
        });
    }
}
