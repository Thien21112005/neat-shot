using NeatShot.Presentation.Controls;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class PinWindowTests
{
    [Fact]
    public void AnnotationToolbar_PinButton_RaisesPinRequestedEvent_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var pinRequested = false;
                toolbar.PinRequested += (s, e) => pinRequested = true;

                Assert.NotNull(toolbar.PinButton);
                toolbar.PinButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                Assert.True(pinRequested);
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
    public void PinWindow_CanBeCreated_AndSetsViewModelImage_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new PinViewModel();
                var window = new PinWindow(vm);

                var dummyBitmap = new RenderTargetBitmap(100, 80, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                window.SetImage(dummyBitmap, new System.Windows.Point(50, 60));

                Assert.NotNull(vm.PinnedImage);
                Assert.Equal(100, vm.OriginalWidth);
                Assert.Equal(80, vm.OriginalHeight);
                Assert.Equal(50, window.Left);
                Assert.Equal(60, window.Top);
                Assert.True(window.Topmost);

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
}
