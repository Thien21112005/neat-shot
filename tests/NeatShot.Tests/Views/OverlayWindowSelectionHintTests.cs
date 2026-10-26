using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayWindowSelectionHintTests
{
    [Fact]
    public void SelectionHint_IsShownUntilARegionIsSelected_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var window = new OverlayWindow(vm, new ExportService(new ScreenCaptureService()));

                // Vừa mở màn hình chụp: hiện gợi ý "kéo để chọn vùng"
                Assert.Equal(Visibility.Visible, window.SelectionHint.Visibility);
                Assert.False(window.SelectionHint.IsHitTestVisible);

                // Đã có vùng chọn: ẩn gợi ý để không che ảnh
                vm.SelectedRegion = new CaptureRegion(10, 10, 200, 120);
                Assert.Equal(Visibility.Collapsed, window.SelectionHint.Visibility);

                // Bỏ vùng chọn (click nhầm / chọn lại): hiện gợi ý trở lại
                vm.SelectedRegion = CaptureRegion.Empty;
                Assert.Equal(Visibility.Visible, window.SelectionHint.Visibility);
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
