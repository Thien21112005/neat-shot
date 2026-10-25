using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowFullScreenSelectionTests
{
    [Fact]
    public void OverlayWindow_SelectFullScreen_SelectsWholeVirtualScreenInWindowCoordinates_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var window = new OverlayWindow(vm, new ExportService(new ScreenCaptureService()));

                // Màn hình phụ nằm bên trái có toạ độ âm; vùng chọn tính theo toạ độ của overlay (gốc 0,0)
                window.SelectFullScreen(new System.Windows.Rect(-1920, 0, 3840, 1080));

                Assert.Equal(new CaptureRegion(0, 0, 3840, 1080), vm.SelectedRegion);
                Assert.True(vm.HasSelection);
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
