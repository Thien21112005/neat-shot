using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
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

public class OverlayWindowSettingsBehaviorTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), "NeatShot_OverlaySettings_" + Guid.NewGuid().ToString("N") + ".json");

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* bỏ qua */ }
    }

    private sealed class FakeExportService : IExportService
    {
        public int CopyCount { get; private set; }
        public string? SavedPath { get; private set; }

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

        public Task SaveToFileAsync(BitmapSource image, string filePath)
        {
            SavedPath = filePath;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGalleryService : IScreenshotGalleryService
    {
        public int SaveCount { get; private set; }
        public string GetScreenshotsDirectory() => @"C:\fake";
        public Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync() => Task.FromResult<IReadOnlyList<GalleryItem>>(Array.Empty<GalleryItem>());
        public Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null)
        {
            SaveCount++;
            return Task.FromResult(@"C:\fake\shot.png");
        }
        public bool DeleteScreenshot(string filePath) => false;
        public BitmapSource? LoadImage(string filePath) => null;
    }

    private SettingsService Settings(AppSettings settings)
    {
        var service = new SettingsService(_settingsPath);
        service.SaveSettings(settings);
        return service;
    }

    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { threadException = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(threadException);
    }

    [Fact]
    public void Copy_DoesNotSaveToGallery_WhenAutoSaveIsTurnedOff_OnStaThread()
    {
        var settings = Settings(new AppSettings { AutoSaveToGallery = false });
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var export = new FakeExportService();
            var gallery = new FakeGalleryService();
            var window = new OverlayWindow(vm, export, settingsService: settings, galleryService: gallery);
            string? copiedPath = "not raised";
            window.CaptureCopied += (s, p) => copiedPath = p;
            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);

            Assert.Equal(1, export.CopyCount);
            Assert.Equal(0, gallery.SaveCount);
            Assert.Null(copiedPath);
        });
    }

    [Fact]
    public void FinishingASelection_CopiesImmediately_WhenAutoCopyIsOn_OnStaThread()
    {
        var settings = Settings(new AppSettings { AutoCopyOnCapture = true });
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var export = new FakeExportService();
            var window = new OverlayWindow(vm, export, settingsService: settings, galleryService: new FakeGalleryService());
            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.SelectionControl.NotifySelectionCompleted();

            Assert.Equal(1, export.CopyCount);
        });
    }

    [Fact]
    public void FinishingASelection_KeepsTheOverlayOpenForEditing_ByDefault_OnStaThread()
    {
        var settings = Settings(new AppSettings());
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var export = new FakeExportService();
            var window = new OverlayWindow(vm, export, settingsService: settings, galleryService: new FakeGalleryService());
            vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

            window.SelectionControl.NotifySelectionCompleted();

            Assert.Equal(0, export.CopyCount);
        });
    }
}
