using NeatShot.Common.Helpers;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ToolbarPositionHelperTests
{
    private readonly Size _toolbarSize = new(640, 44);
    private readonly Size _screenSize = new(1920, 1080);

    [Fact]
    public void CalculatePosition_PlacesBelow_WhenSpaceAvailable()
    {
        // Region in the middle: (200, 200, 500, 300) -> Bottom = 500
        var region = new Rect(200, 200, 500, 300);

        var pos = ToolbarPositionHelper.CalculatePosition(region, _toolbarSize, _screenSize);

        // Should be placed below: Bottom + 8 = 508
        Assert.Equal(508, pos.Y);
        // Right-aligned with region: 700 - 640 = 60 -> clamped to min 8 or left 60
        Assert.Equal(60, pos.X);
    }

    [Fact]
    public void CalculatePosition_PlacesAbove_WhenNoSpaceBelow()
    {
        // Region near bottom: (200, 700, 500, 350) -> Bottom = 1050
        // Below is 1050 + 8 + 44 = 1102 > 1080 (no space below)
        // Above: Top = 700 - 8 - 44 = 648 (space available)
        var region = new Rect(200, 700, 500, 350);

        var pos = ToolbarPositionHelper.CalculatePosition(region, _toolbarSize, _screenSize);

        Assert.Equal(648, pos.Y);
    }

    [Fact]
    public void CalculatePosition_PlacesInsideBottom_WhenFullScreenOrNoSpaceAboveOrBelow()
    {
        // Region almost full screen: (10, 10, 1900, 1060) -> Top = 10, Bottom = 1070
        // No space below (1070 + 52 > 1080)
        // No space above (10 - 52 < 0)
        var region = new Rect(10, 10, 1900, 1060);

        var pos = ToolbarPositionHelper.CalculatePosition(region, _toolbarSize, _screenSize);

        // Should place inside bottom: Bottom - toolbarHeight - 8 = 1070 - 44 - 8 = 1018
        Assert.Equal(1018, pos.Y);

        // Must stay inside screen bounds
        Assert.True(pos.Y >= 4);
        Assert.True(pos.Y + _toolbarSize.Height <= _screenSize.Height - 4);
        Assert.True(pos.X >= 8);
        Assert.True(pos.X + _toolbarSize.Width <= _screenSize.Width - 8);
    }

    [Fact]
    public void CalculatePosition_ClampsHorizontal_WhenNearRightEdge()
    {
        // Region at right edge: (1500, 100, 400, 300) -> Right = 1900
        var region = new Rect(1500, 100, 400, 300);

        var pos = ToolbarPositionHelper.CalculatePosition(region, _toolbarSize, _screenSize);

        // 1900 - 640 = 1260. Clamped to <= 1920 - 640 - 8 = 1272
        Assert.Equal(1260, pos.X);
        Assert.True(pos.X + _toolbarSize.Width <= _screenSize.Width - 8);
    }

    [Fact]
    public void CalculatePosition_ClampsHorizontal_WhenNearLeftEdge()
    {
        // Region at far left: (5, 100, 200, 300) -> Right = 205
        // Right-aligned: 205 - 640 = -435 -> clamped to min 8
        var region = new Rect(5, 100, 200, 300);

        var pos = ToolbarPositionHelper.CalculatePosition(region, _toolbarSize, _screenSize);

        Assert.Equal(8, pos.X);
    }
}
