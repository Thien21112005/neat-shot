using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class WindowHitTesterTests
{
    // Thứ tự z-order: phần tử đầu là cửa sổ nằm trên cùng
    private static readonly IReadOnlyList<WindowRegion> Desktop = new[]
    {
        new WindowRegion(new Rect(100, 100, 400, 300), TopLevelIndex: 0, IsChild: false),   // Cửa sổ A (trên cùng)
        new WindowRegion(new Rect(120, 140, 200, 80), TopLevelIndex: 0, IsChild: true),     // Panel lớn trong A
        new WindowRegion(new Rect(130, 150, 60, 30), TopLevelIndex: 0, IsChild: true),      // Nút nhỏ trong panel
        new WindowRegion(new Rect(300, 200, 500, 400), TopLevelIndex: 1, IsChild: false),   // Cửa sổ B (nằm dưới A)
        new WindowRegion(new Rect(0, 0, 1920, 1080), TopLevelIndex: 2, IsChild: false)      // Desktop
    };

    [Fact]
    public void Find_PicksTheSmallestChildUnderTheCursor_InTheTopmostWindow()
    {
        Assert.Equal(new Rect(130, 150, 60, 30), WindowHitTester.Find(Desktop, new Point(140, 160)));
    }

    [Fact]
    public void Find_PicksTheTopLevelWindow_WhenNoChildContainsTheCursor()
    {
        Assert.Equal(new Rect(100, 100, 400, 300), WindowHitTester.Find(Desktop, new Point(450, 350)));
    }

    [Fact]
    public void Find_RespectsZOrder_WhereWindowsOverlap()
    {
        // (350, 250) nằm trong cả A và B; A ở trên nên được chọn
        Assert.Equal(new Rect(100, 100, 400, 300), WindowHitTester.Find(Desktop, new Point(350, 250)));
        // (700, 500) chỉ nằm trong B
        Assert.Equal(new Rect(300, 200, 500, 400), WindowHitTester.Find(Desktop, new Point(700, 500)));
    }

    [Fact]
    public void Find_FallsBackToTheDesktop_AndReturnsNullOutsideEverything()
    {
        Assert.Equal(new Rect(0, 0, 1920, 1080), WindowHitTester.Find(Desktop, new Point(1800, 1000)));
        Assert.Null(WindowHitTester.Find(Desktop, new Point(5000, 5000)));
        Assert.Null(WindowHitTester.Find(Array.Empty<WindowRegion>(), new Point(10, 10)));
    }

    [Fact]
    public void ToOverlay_ConvertsPhysicalPixelsToOverlayDips()
    {
        // Màn hình ảo bắt đầu ở (-1920, 0) vật lý, tỉ lệ 150%
        var physical = new Rect(-1920 + 300, 150, 600, 300);

        var dip = WindowHitTester.ToOverlay(physical, virtualOriginX: -1920, virtualOriginY: 0, scaleX: 1.5, scaleY: 1.5);

        Assert.Equal(new Rect(200, 100, 400, 200), dip);
    }
}
