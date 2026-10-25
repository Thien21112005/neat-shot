using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowShortcutTests
{
    private sealed class PendingOcrService : IOcrService
    {
        public int CallCount { get; private set; }

        public bool IsOcrSupported() => true;

        public Task<string> RecognizeTextAsync(BitmapSource image, CancellationToken cancellationToken = default)
        {
            CallCount++;
            // Không bao giờ hoàn tất để tránh hiển thị MessageBox/Clipboard trong môi trường test
            return new TaskCompletionSource<string>().Task;
        }
    }

    [Fact]
    public void OverlayWindow_CtrlO_TriggersTextExtraction_InsteadOfEllipseTool_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var exportService = new ExportService(new ScreenCaptureService());
                var ocr = new PendingOcrService();
                var window = new OverlayWindow(vm, exportService, ocr);

                vm.BackgroundImage = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
                vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

                var handled = window.ProcessShortcut(Key.O, ModifierKeys.Control);

                Assert.True(handled);
                Assert.Equal(1, ocr.CallCount);
                Assert.Equal(DrawingToolType.None, window.DrawingControl.CurrentTool);
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
    public void OverlayWindow_PlainO_StillSelectsEllipseTool_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var exportService = new ExportService(new ScreenCaptureService());
                var ocr = new PendingOcrService();
                var window = new OverlayWindow(vm, exportService, ocr);
                vm.SelectedRegion = new CaptureRegion(10, 10, 100, 80);

                Assert.True(window.ProcessShortcut(Key.O, ModifierKeys.None));
                Assert.Equal(DrawingToolType.Ellipse, window.DrawingControl.CurrentTool);
                Assert.Equal(0, ocr.CallCount);
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
}
