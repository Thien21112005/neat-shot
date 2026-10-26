using NeatShot.Core.Models;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Models;

public class CaptureRegionClampTests
{
    private static readonly Size Screen = new(1920, 1080);

    [Fact]
    public void ClampTo_KeepsARegionThatAlreadyFits()
    {
        var region = new CaptureRegion(100, 200, 640, 360);
        Assert.Equal(region, region.ClampTo(Screen));
    }

    [Fact]
    public void ClampTo_TrimsTheParts_OutsideTheScreen()
    {
        // Màn hình phụ đã bị rút ra: vùng cũ tràn ra ngoài
        var region = new CaptureRegion(1700, 900, 600, 400);
        Assert.Equal(new CaptureRegion(1700, 900, 220, 180), region.ClampTo(Screen));
    }

    [Fact]
    public void ClampTo_ReturnsEmpty_WhenTheRegionIsCompletelyOffScreen()
    {
        Assert.True(new CaptureRegion(2500, 100, 300, 200).ClampTo(Screen).IsEmpty);
    }

    [Fact]
    public void ClampTo_NormalizesReversedRegions()
    {
        Assert.Equal(new CaptureRegion(100, 100, 200, 100), new CaptureRegion(300, 200, -200, -100).ClampTo(Screen));
    }
}
