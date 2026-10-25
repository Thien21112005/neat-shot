using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Xunit;

namespace NeatShot.Tests.Views;

public class PinWindowZoomAnnotationTests
{
    [Fact]
    public void PinWindow_Zoom_KeepsDrawingCanvasInImageCoordinates_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new PinWindow(new PinViewModel());
                window.SetImage(new RenderTargetBitmap(200, 100, 96, 96, PixelFormats.Pbgra32), new Point(0, 0));

                window.ViewModel.ZoomIn();
                window.ViewModel.ZoomIn();
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);

                // Canvas vẽ phải giữ hệ toạ độ của ảnh gốc; phóng to chỉ là phép biến đổi hiển thị,
                // nhờ vậy nét vẽ không bị trôi khi zoom và ảnh xuất ra không cần quy đổi tỉ lệ.
                Assert.Equal(200, window.DrawingControl.Width);
                Assert.Equal(100, window.DrawingControl.Height);
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
