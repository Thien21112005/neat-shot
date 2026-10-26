using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowRepeatRegionTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), "NeatShot_Repeat_" + Guid.NewGuid().ToString("N") + ".json");

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* bỏ qua */ }
    }

    private sealed class FakeExportService : IExportService
    {
        public RenderTargetBitmap RenderFinalImage(BitmapSource background, CaptureRegion region, IEnumerable<DrawingElement> annotations, BeautifyOptions? beautifyOptions = null)
        {
            var rtb = new RenderTargetBitmap(10, 10, 96, 96, PixelFormats.Pbgra32);
            rtb.Freeze();
            return rtb;
        }

        public Task CopyToClipboardAsync(BitmapSource image) => Task.CompletedTask;
        public Task SaveToFileAsync(BitmapSource image, string filePath) => Task.CompletedTask;
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
    public void Copying_RemembersTheRegion_ForRepeatCapture_OnStaThread()
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings());

        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var window = new OverlayWindow(vm, new FakeExportService(), settingsService: settings);
            vm.BackgroundImage = new RenderTargetBitmap(800, 600, 96, 96, PixelFormats.Pbgra32);
            vm.SelectedRegion = new CaptureRegion(40, 50, 320, 200);

            window.ProcessShortcut(Key.Enter, ModifierKeys.None);
        });

        Assert.Equal(new CaptureRegion(40, 50, 320, 200), new SettingsService(_settingsPath).LoadSettings().LastCaptureRegion);
    }

    [Fact]
    public void PreselectRegion_SelectsTheRememberedRegion_ClampedToTheScreen_OnStaThread()
    {
        RunOnSta(() =>
        {
            var vm = new OverlayViewModel();
            var window = new OverlayWindow(vm, new ExportService(new ScreenCaptureService()));

            window.PreselectRegion(new CaptureRegion(1700, 900, 600, 400), new Rect(0, 0, 1920, 1080));

            Assert.Equal(new CaptureRegion(1700, 900, 220, 180), vm.SelectedRegion);
        });
    }
}
