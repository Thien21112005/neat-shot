using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
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

public class OverlayWindowLivePreviewAndOcrTests
{
    [Fact]
    public void OverlayWindow_LiveBeautifyPreview_UpdatesOnPresetSelection_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                // Ban đầu không có vùng chọn, BeautifyPreviewFrame ẩn
                Assert.Equal(Visibility.Collapsed, window.BeautifyPreviewFrame.Visibility);

                // Thiết lập vùng chọn
                vm.SelectedRegion = new CaptureRegion(100, 100, 200, 150);

                // Mặc định là preset None, BeautifyPreviewFrame vẫn ẩn
                Assert.Equal(Visibility.Collapsed, window.BeautifyPreviewFrame.Visibility);

                // Chọn preset Ocean
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.Ocean);

                // BeautifyPreviewFrame phải hiển thị và mở rộng thêm Padding 20px ở cả 2 phía (viền mỏng tinh tế)
                Assert.Equal(Visibility.Visible, window.BeautifyPreviewFrame.Visibility);
                Assert.Equal(200 + 40, window.BeautifyPreviewFrame.Width);
                Assert.Equal(150 + 40, window.BeautifyPreviewFrame.Height);

                // Chọn lại preset None (Gốc)
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.None);
                Assert.Equal(Visibility.Collapsed, window.BeautifyPreviewFrame.Visibility);
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
    public void OverlayWindow_LiveBeautifyPreview_RendersScreenshotImageInsideInnerPhotoFrame_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var dummyBitmap = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
                vm.BackgroundImage = dummyBitmap;
                vm.SelectedRegion = new CaptureRegion(50, 50, 200, 150);

                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                // Chọn preset Sunset
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.Sunset);

                // Khung làm đẹp ngoài hiển thị
                Assert.Equal(Visibility.Visible, window.BeautifyPreviewFrame.Visibility);

                // Khung ảnh bên trong phải chứa ImageBrush của ảnh chụp thực tế
                Assert.NotNull(window.BeautifyInnerPhotoFrame.Background);
                var imageBrush = Assert.IsType<ImageBrush>(window.BeautifyInnerPhotoFrame.Background);
                Assert.NotNull(imageBrush.ImageSource);
                Assert.Equal(200, window.BeautifyInnerPhotoFrame.Width);
                Assert.Equal(150, window.BeautifyInnerPhotoFrame.Height);

                // Đổi về preset None -> Ẩn và giải phóng background của khung ảnh
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.None);
                Assert.Equal(Visibility.Collapsed, window.BeautifyPreviewFrame.Visibility);
                Assert.Null(window.BeautifyInnerPhotoFrame.Background);
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
    public void OverlayWindow_LiveBeautifyPreview_ShowsResizeHandlesOnOverlay_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var dummyBitmap = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
                vm.BackgroundImage = dummyBitmap;
                vm.SelectedRegion = new CaptureRegion(50, 50, 200, 150);

                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                // Ban đầu chưa chọn preset -> BeautifyHandlesLayer ẩn
                Assert.Equal(Visibility.Collapsed, window.BeautifyHandlesLayer.Visibility);

                // Chọn preset Sunset
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.Sunset);

                // BeautifyHandlesLayer phải hiển thị trên khung review
                Assert.Equal(Visibility.Visible, window.BeautifyHandlesLayer.Visibility);
                Assert.Equal(50 - 4, Canvas.GetLeft(window.BHandleNW));
                Assert.Equal(50 - 4, Canvas.GetTop(window.BHandleNW));
                Assert.Equal(250 - 4, Canvas.GetLeft(window.BHandleSE));
                Assert.Equal(200 - 4, Canvas.GetTop(window.BHandleSE));

                // Đổi về preset None -> BeautifyHandlesLayer ẩn đi
                window.Toolbar.SelectBeautifyPreset(BeautifyPreset.None);
                Assert.Equal(Visibility.Collapsed, window.BeautifyHandlesLayer.Visibility);
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
    public void OverlayWindow_OcrResultCard_EscapeClosesCardWithoutExitingOverlay_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                // Ban đầu OcrResultCard ẩn
                Assert.Equal(Visibility.Collapsed, window.OcrResultCard.Visibility);

                // Mở OcrResultCard
                window.OcrResultTextBox.Text = "Hello World OCR";
                window.OcrResultCard.Visibility = Visibility.Visible;

                // Nhấn phím Escape
                var handled = window.ProcessShortcut(Key.Escape, ModifierKeys.None);

                // Card phải được đóng, và phím Escape được đánh dấu đã xử lý
                Assert.True(handled);
                Assert.Equal(Visibility.Collapsed, window.OcrResultCard.Visibility);
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
