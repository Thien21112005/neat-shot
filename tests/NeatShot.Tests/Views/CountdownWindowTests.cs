using NeatShot.Core.Models;
using NeatShot.Presentation.Views;
using System.Threading;
using Xunit;

namespace NeatShot.Tests.Views;

public class CountdownWindowTests
{
    [Fact]
    public void CountdownWindow_ShowsTheRemainingSeconds_AndNeverStealsFocus_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var countdown = new CaptureCountdown(3);
                var window = new CountdownWindow(countdown);

                Assert.Equal("3", window.SecondsText.Text);
                Assert.False(window.ShowActivated);
                Assert.True(window.Topmost);
                Assert.False(window.ShowInTaskbar);

                countdown.Tick();
                Assert.Equal("2", window.SecondsText.Text);

                countdown.Tick();
                Assert.Equal("1", window.SecondsText.Text);
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
    public void CancelButton_CancelsTheCountdown_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var countdown = new CaptureCountdown(5);
                var cancelled = 0;
                countdown.Cancelled += (s, e) => cancelled++;
                var window = new CountdownWindow(countdown);

                window.CancelButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                Assert.Equal(1, cancelled);
                Assert.True(countdown.IsFinished);
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
