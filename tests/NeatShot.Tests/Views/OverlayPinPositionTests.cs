using NeatShot.Core.Models;
using NeatShot.Presentation.Views;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Views;

public class OverlayPinPositionTests
{
    [Fact]
    public void CalculatePinPosition_WithoutBeautify_AlignsWithSelection()
    {
        var pos = OverlayWindow.CalculatePinPosition(
            new Point(-1920, 0),
            new CaptureRegion(100, 50, 300, 200),
            beautifyOptions: null);

        Assert.Equal(new Point(-1820, 50), pos);
    }

    [Fact]
    public void CalculatePinPosition_WithBeautify_OffsetsByPadding_SoImageStaysAlignedWithSelection()
    {
        var options = new BeautifyOptions { IsEnabled = true, Preset = BeautifyPreset.Ocean, Padding = 20 };

        var pos = OverlayWindow.CalculatePinPosition(
            new Point(0, 0),
            new CaptureRegion(100, 50, 300, 200),
            options);

        // Ảnh làm đẹp có thêm 20px đệm mỗi phía -> cửa sổ ghim phải lùi 20px để phần ảnh gốc trùng vùng chọn
        Assert.Equal(new Point(80, 30), pos);
    }

    [Fact]
    public void CalculatePinPosition_WithPresetNone_IgnoresPadding()
    {
        var options = new BeautifyOptions { IsEnabled = true, Preset = BeautifyPreset.None, Padding = 20 };

        var pos = OverlayWindow.CalculatePinPosition(new Point(0, 0), new CaptureRegion(100, 50, 300, 200), options);

        Assert.Equal(new Point(100, 50), pos);
    }
}
