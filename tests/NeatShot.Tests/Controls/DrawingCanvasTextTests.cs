using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Controls;

public class DrawingCanvasTextTests
{
    [Fact]
    public void CommitText_AddsTextElementWithCustomFontFamily_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();
                canvas.CurrentTool = DrawingToolType.Text;
                canvas.CurrentColor = Colors.Yellow;

                canvas.CommitText("Hello Times New Roman", new Point(50, 60), fontSize: 22.0, fontFamily: "Times New Roman");

                Assert.Single(canvas.UndoStack.Items);
                var item = canvas.UndoStack.Items.First();
                Assert.Equal(DrawingToolType.Text, item.ToolType);
                Assert.Equal("Hello Times New Roman", item.Text);
                Assert.Equal(Colors.Yellow, item.Color);
                Assert.Equal(22.0, item.FontSize);
                Assert.Equal("Times New Roman", item.FontFamily);
                Assert.Equal(50, item.StartPoint.X);
                Assert.Equal(60, item.StartPoint.Y);
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
    public void CommitText_UsesCanvasDefaultFontFamilyAndSize_WhenNotSpecified_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();
                canvas.CurrentTool = DrawingToolType.Text;
                canvas.CurrentFontFamily = "Arial";
                canvas.CurrentFontSize = 18.0;

                canvas.CommitText("Default Font Note", new Point(100, 100));

                Assert.Single(canvas.UndoStack.Items);
                var item = canvas.UndoStack.Items.First();
                Assert.Equal("Default Font Note", item.Text);
                Assert.Equal("Arial", item.FontFamily);
                Assert.Equal(18.0, item.FontSize);
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
    public void CommitText_IgnoresEmptyOrWhitespaceText_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();
                canvas.CommitText("", new Point(10, 10));
                canvas.CommitText("   \t  ", new Point(20, 20));

                Assert.Empty(canvas.UndoStack.Items);
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
    public void DrawingRenderer_RendersText_WithDifferentFontFamilies_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var timesElement = new DrawingElement
                {
                    ToolType = DrawingToolType.Text,
                    StartPoint = new Point(10, 10),
                    Text = "Times text",
                    FontFamily = "Times New Roman",
                    FontSize = 16.0,
                    Color = Colors.White
                };
                var arialElement = new DrawingElement
                {
                    ToolType = DrawingToolType.Text,
                    StartPoint = new Point(10, 40),
                    Text = "Arial text",
                    FontFamily = "Arial",
                    FontSize = 14.0,
                    Color = Colors.Red
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, timesElement);
                    DrawingRenderer.RenderElement(dc, arialElement);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count >= 2);
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
