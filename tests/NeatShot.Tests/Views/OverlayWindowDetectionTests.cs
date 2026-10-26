using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowDetectionTests
{
    [Fact]
    public void Overlay_WiresDetectedWindows_IntoTheSelectionCanvas_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var window = new OverlayWindow(vm, new ExportService(new ScreenCaptureService()));
                window.SelectionControl.Measure(new Size(1200, 800));
                window.SelectionControl.Arrange(new Rect(0, 0, 1200, 800));
                // Cho binding SelectedRegion (control -> ViewModel) được gắn như khi cửa sổ hiển thị thật
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.DataBind);

                window.SetWindowRegions(new[]
                {
                    new WindowRegion(new Rect(100, 100, 500, 300), 0, false),
                    new WindowRegion(new Rect(0, 0, 1200, 800), 1, false)
                });

                window.SelectionControl.HandlePointerMove(new Point(200, 200));
                Assert.Equal(new Rect(100, 100, 500, 300), window.SelectionControl.HoveredRegion);

                window.SelectionControl.HandlePointerDown(new Point(200, 200));
                window.SelectionControl.HandlePointerUp(new Point(200, 200));
                Assert.Equal(new CaptureRegion(100, 100, 500, 300), vm.SelectedRegion);
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
