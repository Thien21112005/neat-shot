using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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
