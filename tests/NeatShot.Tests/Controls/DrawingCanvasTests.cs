using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Controls;

public class DrawingCanvasTests
{
    [Fact]
    public void DrawingCanvas_ToolSwitching_UpdatesHitTestAndCursor_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();

                // Initial state: tool is None -> not hit test visible
                Assert.Equal(DrawingToolType.None, canvas.CurrentTool);
                Assert.False(canvas.IsHitTestVisible);

                // Switch to Pencil -> hit test visible, Pen cursor
                canvas.CurrentTool = DrawingToolType.Pencil;
                Assert.True(canvas.IsHitTestVisible);
                Assert.Equal(Cursors.Pen, canvas.Cursor);

                // Switch to Rectangle -> hit test visible, Cross cursor
                canvas.CurrentTool = DrawingToolType.Rectangle;
                Assert.True(canvas.IsHitTestVisible);
                Assert.Equal(Cursors.Cross, canvas.Cursor);

                // Switch back to None -> not hit test visible, Arrow cursor
                canvas.CurrentTool = DrawingToolType.None;
                Assert.False(canvas.IsHitTestVisible);
                Assert.Equal(Cursors.Arrow, canvas.Cursor);
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
    public void OffsetElements_ShiftsExistingDrawingElements_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();
                var element = new DrawingElement
                {
                    ToolType = DrawingToolType.Rectangle,
                    StartPoint = new Point(10, 10),
                    EndPoint = new Point(50, 50),
                    Rect = new Rect(10, 10, 40, 40)
                };
                canvas.UndoStack.Push(element);

                // Act: Offset by dx = 15, dy = 25
                canvas.OffsetElements(15, 25);

                // Assert
                var item = canvas.UndoStack.Items.First();
                Assert.Equal(25, item.StartPoint.X);
                Assert.Equal(35, item.StartPoint.Y);
                Assert.Equal(65, item.EndPoint.X);
                Assert.Equal(75, item.EndPoint.Y);
                Assert.Equal(25, item.Rect.X);
                Assert.Equal(35, item.Rect.Y);
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
