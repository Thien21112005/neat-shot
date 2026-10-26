using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class PinWindowHudTests
{
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
    public void PinViewModel_HudText_ReflectsZoomAndOpacity()
    {
        var vm = new PinViewModel();
        Assert.Equal("Zoom 100% · Opacity 100%", vm.HudText);

        vm.ZoomIn();
        vm.ZoomIn();
        vm.DecreaseOpacity();
        vm.DecreaseOpacity();
        vm.DecreaseOpacity();

        Assert.Equal("Zoom 120% · Opacity 70%", vm.HudText);
    }

    [Fact]
    public void PinWindow_ShowsHud_OnlyWhileZoomOrOpacityChanges_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new PinWindow(new PinViewModel());
            window.SetImage(new RenderTargetBitmap(200, 100, 96, 96, PixelFormats.Pbgra32), new Point(0, 0));

            // Vừa ghim xong: không hiện HUD
            Assert.Equal(Visibility.Collapsed, window.PinHud.Visibility);

            window.ViewModel.ZoomIn();

            Assert.Equal(Visibility.Visible, window.PinHud.Visibility);
            Assert.Equal("Zoom 110% · Opacity 100%", window.PinHudText.Text);
            window.Close();
        });
    }
}
