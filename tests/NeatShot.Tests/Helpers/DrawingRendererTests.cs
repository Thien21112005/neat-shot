using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class DrawingRendererTests
{
    [Fact]
    public void RenderElement_ThrowsArgumentNullException_WhenDcOrElementIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => DrawingRenderer.RenderElement(null!, new DrawingElement()));
    }

    [Fact]
    public void RenderElement_RendersEllipse_ProducesDrawing_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var ellipse = new DrawingElement
                {
                    ToolType = DrawingToolType.Ellipse,
                    Rect = new Rect(10, 10, 80, 50),
                    Color = Colors.Red,
                    Thickness = 2.0
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, ellipse);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count > 0, "Ellipse must produce at least one drawing child");
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }

    [Fact]
    public void RenderElement_RendersLine_ProducesDrawing_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var line = new DrawingElement
                {
                    ToolType = DrawingToolType.Line,
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(100, 50),
                    Color = Colors.Blue,
                    Thickness = 3.0
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, line);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count > 0, "Line must produce at least one drawing child");
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }

    [Fact]
    public void RenderElement_RendersHighlight_ProducesSemiTransparentDrawing_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var highlight = new DrawingElement
                {
                    ToolType = DrawingToolType.Highlight,
                    Points = new List<Point> { new(10, 10), new(50, 10), new(100, 10) },
                    Color = Colors.Yellow,
                    Thickness = 4.0
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, highlight);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count > 0, "Highlight must produce at least one drawing child");
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }

    [Fact]
    public void RenderElement_RendersText_ProducesDrawing_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var text = new DrawingElement
                {
                    ToolType = DrawingToolType.Text,
                    StartPoint = new Point(20, 20),
                    Text = "Hello NeatShot",
                    Color = Colors.White,
                    FontSize = 18.0
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, text);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count > 0, "Text must produce at least one drawing child");
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }

    [Fact]
    public void RenderElement_SkipsText_WhenTextIsNullOrWhitespace_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var emptyText = new DrawingElement
                {
                    ToolType = DrawingToolType.Text,
                    StartPoint = new Point(20, 20),
                    Text = "   ",
                    Color = Colors.White
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, emptyText);
                }

                // Empty text should not produce drawings
                var count = visual.Drawing?.Children.Count ?? 0;
                Assert.Equal(0, count);
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }
}
