using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Models;

public class DrawingElementTests
{
    [Fact]
    public void DrawingElement_Defaults_AreInitializedProperly()
    {
        var element = new DrawingElement();

        Assert.Equal(DrawingToolType.None, element.ToolType);
        Assert.Equal(Colors.Red, element.Color);
        Assert.Equal(3.0, element.Thickness);
        Assert.Equal(16.0, element.FontSize);
        Assert.NotNull(element.Points);
        Assert.Empty(element.Points);
        Assert.Null(element.Text);
    }

    [Fact]
    public void DrawingToolType_ContainsAllExpectedTools()
    {
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.None));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Pencil));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Rectangle));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Ellipse));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Line));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Arrow));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Highlight));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Text));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Select));
        Assert.True(Enum.IsDefined(typeof(DrawingToolType), DrawingToolType.Eyedropper));
    }

    [Fact]
    public void GetBoundingBox_ForRectangleAndEllipse_ReturnsRect()
    {
        var rectElem = new DrawingElement
        {
            ToolType = DrawingToolType.Rectangle,
            Rect = new Rect(10, 20, 100, 50)
        };
        Assert.Equal(new Rect(10, 20, 100, 50), rectElem.GetBoundingBox());

        var ellipseElem = new DrawingElement
        {
            ToolType = DrawingToolType.Ellipse,
            Rect = new Rect(30, 40, 80, 60)
        };
        Assert.Equal(new Rect(30, 40, 80, 60), ellipseElem.GetBoundingBox());
    }

    [Fact]
    public void GetBoundingBox_ForLineAndArrow_CalculatesNormalizedRect()
    {
        var lineElem = new DrawingElement
        {
            ToolType = DrawingToolType.Line,
            StartPoint = new Point(100, 200),
            EndPoint = new Point(50, 80)
        };
        Assert.Equal(new Rect(50, 80, 50, 120), lineElem.GetBoundingBox());
    }

    [Fact]
    public void GetBoundingBox_ForPoints_EnclosesAllPoints()
    {
        var pencil = new DrawingElement
        {
            ToolType = DrawingToolType.Pencil,
            Points = new List<Point>
            {
                new(10, 50),
                new(100, 20),
                new(40, 90)
            }
        };

        var box = pencil.GetBoundingBox();
        Assert.Equal(10, box.X);
        Assert.Equal(20, box.Y);
        Assert.Equal(90, box.Width);
        Assert.Equal(70, box.Height);
    }
}
