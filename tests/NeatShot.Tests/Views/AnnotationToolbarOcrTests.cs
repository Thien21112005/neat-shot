using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xunit;

namespace NeatShot.Tests.Views;

public class AnnotationToolbarOcrTests
{
    [Fact]
    public void AnnotationToolbar_OcrButton_RaisesOcrRequestedEvent_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var ocrRequested = false;
                toolbar.OcrRequested += (s, e) => ocrRequested = true;

                Assert.NotNull(toolbar.OcrButton);
                toolbar.OcrButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                Assert.True(ocrRequested);
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
