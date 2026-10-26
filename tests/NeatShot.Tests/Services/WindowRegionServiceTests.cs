using NeatShot.Core.Services.Implementations;
using Xunit;

namespace NeatShot.Tests.Services;

public class WindowRegionServiceTests
{
    [Fact]
    public void GetVisibleWindows_ReturnsRealWindows_InZOrder_WithUsableBounds()
    {
        var regions = new WindowRegionService().GetVisibleWindows();

        // Luôn có ít nhất taskbar / desktop trên một phiên Windows có giao diện
        Assert.NotEmpty(regions);
        Assert.All(regions, r => Assert.True(r.Bounds.Width >= 8 && r.Bounds.Height >= 8));

        var topLevelOrder = regions.Where(r => !r.IsChild).Select(r => r.TopLevelIndex).ToList();
        Assert.Equal(topLevelOrder.OrderBy(i => i), topLevelOrder);
    }

    [Fact]
    public void ChildRegions_StayInsideTheirParentWindow()
    {
        var regions = new WindowRegionService().GetVisibleWindows();
        var parents = regions.Where(r => !r.IsChild).ToDictionary(r => r.TopLevelIndex, r => r.Bounds);

        Assert.All(regions.Where(r => r.IsChild), child =>
            Assert.True(parents[child.TopLevelIndex].Contains(child.Bounds)));
    }
}
