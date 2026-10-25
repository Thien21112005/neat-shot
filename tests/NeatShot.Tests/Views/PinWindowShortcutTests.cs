using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class PinWindowShortcutTests
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

    private static PinWindow CreateWindow()
    {
        var window = new PinWindow(new PinViewModel());
        window.SetImage(new RenderTargetBitmap(200, 100, 96, 96, PixelFormats.Pbgra32), new Point(0, 0));
        return window;
    }

    [Theory]
    [InlineData(Key.P)]
    [InlineData(Key.OemPlus)]
    [InlineData(Key.OemMinus)]
    [InlineData(Key.Add)]
    [InlineData(Key.Subtract)]
    public void PinWindow_TypingInTextEditor_DoesNotTriggerShortcuts_OnStaThread(Key key)
    {
        RunOnSta(() =>
        {
            var window = CreateWindow();

            var handled = window.ProcessShortcut(key, ModifierKeys.None, new TextBox());

            Assert.False(handled);
            Assert.Equal(1.0, window.ViewModel.Scale);
            Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);
            window.Close();
        });
    }

    [Fact]
    public void PinWindow_ShortcutsStillWork_WhenNotTyping_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = CreateWindow();

            Assert.True(window.ProcessShortcut(Key.OemPlus, ModifierKeys.None));
            Assert.Equal(1.1, window.ViewModel.Scale, 3);

            Assert.True(window.ProcessShortcut(Key.P, ModifierKeys.None));
            Assert.Equal(Visibility.Visible, window.AnnotationToolbar.Visibility);
            window.Close();
        });
    }
}
