using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Controls;

public class DrawingCanvasDegenerateStrokeTests
{
    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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

    [Theory]
    [InlineData(DrawingToolType.Pencil)]
    [InlineData(DrawingToolType.Highlight)]
    [InlineData(DrawingToolType.Rectangle)]
    [InlineData(DrawingToolType.Ellipse)]
    [InlineData(DrawingToolType.Line)]
    [InlineData(DrawingToolType.Arrow)]
    public void ClickWithoutDrag_DoesNotAddInvisibleElementToUndoStack(DrawingToolType tool)
    {
        RunOnSta(() =>
        {
            var canvas = new DrawingCanvas { CurrentTool = tool };

            canvas.StartDrawing(new Point(50, 50));
            canvas.EndDrawing();

            // Nét vẽ rỗng không hiển thị gì nhưng sẽ "nuốt" một lần Ctrl+Z của người dùng
            Assert.Equal(0, canvas.UndoStack.Count);
        });
    }

    [Fact]
    public void ClickWithoutDrag_StillPlacesStepCounterBadge()
    {
        RunOnSta(() =>
        {
            var canvas = new DrawingCanvas { CurrentTool = DrawingToolType.StepCounter };

            canvas.StartDrawing(new Point(50, 50));
            canvas.EndDrawing();

            Assert.Equal(1, canvas.UndoStack.Count);
        });
    }

    [Theory]
    [InlineData(DrawingToolType.Pencil)]
    [InlineData(DrawingToolType.Rectangle)]
    [InlineData(DrawingToolType.Arrow)]
    public void DragStroke_IsStillCommitted(DrawingToolType tool)
    {
        RunOnSta(() =>
        {
            var canvas = new DrawingCanvas { CurrentTool = tool };

            canvas.StartDrawing(new Point(10, 10));
            canvas.MoveDrawing(new Point(60, 40));
            canvas.EndDrawing();

            Assert.Equal(1, canvas.UndoStack.Count);
        });
    }
}
