using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class HitTestHelperTests
{
    [Fact]
    public void HitTest_ThrowsArgumentNullException_WhenElementIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => HitTestHelper.HitTest(null!, new Point(10, 10)));
    }

    [Fact]
    public void HitTest_Rectangle_ReturnsTrue_WhenPointInsideOrOnBorder()
    {
        var element = new DrawingElement
        {
            ToolType = DrawingToolType.Rectangle,
            Rect = new Rect(10, 10, 100, 50),
            Thickness = 3
        };

        // On border / corner
        Assert.True(HitTestHelper.HitTest(element, new Point(10, 10), 6.0));
        // Inside
        Assert.True(HitTestHelper.HitTest(element, new Point(50, 30), 6.0));
        // Just outside within tolerance
        Assert.True(HitTestHelper.HitTest(element, new Point(7, 10), 6.0));
        // Far outside
        Assert.False(HitTestHelper.HitTest(element, new Point(200, 200), 6.0));
    }

    [Fact]
    public void HitTest_Ellipse_ReturnsTrue_WhenPointInsideOrNearPerimeter()
    {
        var ellipse = new DrawingElement
        {
            ToolType = DrawingToolType.Ellipse,
            Rect = new Rect(0, 0, 100, 60), // Center is (50, 30), rx=50, ry=30
            Thickness = 2
        };

        // Center
        Assert.True(HitTestHelper.HitTest(ellipse, new Point(50, 30), 6.0));
        // Near edge
        Assert.True(HitTestHelper.HitTest(ellipse, new Point(99, 30), 6.0));
        // Outside
        Assert.False(HitTestHelper.HitTest(ellipse, new Point(100, 100), 6.0));
    }

    [Fact]
    public void HitTest_Line_IdentifiesHitsCorrectly()
    {
        var line = new DrawingElement
        {
            ToolType = DrawingToolType.Line,
            StartPoint = new Point(0, 0),
            EndPoint = new Point(100, 0),
            Thickness = 2
        };

        // Directly on line
        Assert.True(HitTestHelper.HitTest(line, new Point(50, 0), 6.0));
        // Near line within tolerance
        Assert.True(HitTestHelper.HitTest(line, new Point(50, 3), 6.0));
        // Outside tolerance
        Assert.False(HitTestHelper.HitTest(line, new Point(50, 20), 6.0));
        // Beyond endpoints
        Assert.False(HitTestHelper.HitTest(line, new Point(120, 0), 6.0));
    }

    [Fact]
    public void HitTest_Arrow_HitsShaftAndHead()
    {
        var arrow = new DrawingElement
        {
            ToolType = DrawingToolType.Arrow,
            StartPoint = new Point(0, 50),
            EndPoint = new Point(100, 50),
            Thickness = 3
        };

        // On arrow shaft
        Assert.True(HitTestHelper.HitTest(arrow, new Point(40, 50), 6.0));
        // Near tip
        Assert.True(HitTestHelper.HitTest(arrow, new Point(98, 50), 6.0));
        // Far away
        Assert.False(HitTestHelper.HitTest(arrow, new Point(40, 90), 6.0));
    }

    [Fact]
    public void HitTest_PencilAndHighlight_HitsPolylineSegments()
    {
        var highlight = new DrawingElement
        {
            ToolType = DrawingToolType.Highlight,
            Points = new List<Point>
            {
                new(10, 10),
                new(50, 10),
                new(50, 80)
            },
            Thickness = 16
        };

        // On first segment
        Assert.True(HitTestHelper.HitTest(highlight, new Point(30, 12), 6.0));
        // On second segment
        Assert.True(HitTestHelper.HitTest(highlight, new Point(52, 40), 6.0));
        // Away from segments
        Assert.False(HitTestHelper.HitTest(highlight, new Point(20, 50), 6.0));
    }

    [Fact]
    public void HitTest_Text_HitsBoundingBox()
    {
        var text = new DrawingElement
        {
            ToolType = DrawingToolType.Text,
            StartPoint = new Point(100, 100),
            Rect = new Rect(100, 100, 80, 25),
            Text = "Hello"
        };

        Assert.True(HitTestHelper.HitTest(text, new Point(120, 110), 6.0));
        Assert.False(HitTestHelper.HitTest(text, new Point(50, 50), 6.0));
    }
}
