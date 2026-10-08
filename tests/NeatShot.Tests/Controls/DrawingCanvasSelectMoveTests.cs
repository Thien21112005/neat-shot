using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Controls;

public class DrawingCanvasSelectMoveTests
{
    [Fact]
    public void SelectElementAt_SelectsTopmostElementUnderCursor_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();

                var rect = new DrawingElement
                {
                    ToolType = DrawingToolType.Rectangle,
                    Rect = new Rect(20, 20, 50, 50),
                    StartPoint = new Point(20, 20),
                    EndPoint = new Point(70, 70)
                };
                var ellipse = new DrawingElement
                {
                    ToolType = DrawingToolType.Ellipse,
                    Rect = new Rect(100, 100, 40, 40),
                    StartPoint = new Point(100, 100),
                    EndPoint = new Point(140, 140)
                };

                canvas.UndoStack.Push(rect);
                canvas.UndoStack.Push(ellipse);

                // 1. Click over ellipse
                var hitEllipse = canvas.SelectElementAt(new Point(120, 120));
                Assert.True(hitEllipse);
                Assert.Same(ellipse, canvas.SelectedElement);

                // 2. Click over rect
                var hitRect = canvas.SelectElementAt(new Point(30, 30));
                Assert.True(hitRect);
                Assert.Same(rect, canvas.SelectedElement);

                // 3. Click empty space
                var hitNone = canvas.SelectElementAt(new Point(500, 500));
                Assert.False(hitNone);
                Assert.Null(canvas.SelectedElement);
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
    public void MoveSelectedElement_TranslatesRectAndPoints_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();

                var rect = new DrawingElement
                {
                    ToolType = DrawingToolType.Rectangle,
                    Rect = new Rect(20, 20, 50, 50),
                    StartPoint = new Point(20, 20),
                    EndPoint = new Point(70, 70)
                };
                canvas.UndoStack.Push(rect);

                canvas.SelectElementAt(new Point(25, 25));
                Assert.NotNull(canvas.SelectedElement);

                // Act: Move by dx = 15, dy = 25
                canvas.MoveSelectedElement(15, 25);

                Assert.Equal(35, canvas.SelectedElement.Rect.X);
                Assert.Equal(45, canvas.SelectedElement.Rect.Y);
                Assert.Equal(35, canvas.SelectedElement.StartPoint.X);
                Assert.Equal(45, canvas.SelectedElement.StartPoint.Y);
                Assert.Equal(85, canvas.SelectedElement.EndPoint.X);
                Assert.Equal(95, canvas.SelectedElement.EndPoint.Y);
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
    public void MoveSelectedElement_TranslatesPolylinePoints_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();

                var highlight = new DrawingElement
                {
                    ToolType = DrawingToolType.Highlight,
                    Points = new List<Point> { new(10, 10), new(50, 10) },
                    Thickness = 16
                };
                canvas.UndoStack.Push(highlight);

                canvas.SelectElementAt(new Point(30, 10));
                Assert.NotNull(canvas.SelectedElement);

                // Act: Move by dx = 20, dy = -5
                canvas.MoveSelectedElement(20, -5);

                Assert.Equal(30, canvas.SelectedElement.Points[0].X);
                Assert.Equal(5, canvas.SelectedElement.Points[0].Y);
                Assert.Equal(70, canvas.SelectedElement.Points[1].X);
                Assert.Equal(5, canvas.SelectedElement.Points[1].Y);
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
    public void DeleteSelectedElement_RemovesElementFromUndoStack_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();

                var line = new DrawingElement
                {
                    ToolType = DrawingToolType.Line,
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(100, 100)
                };
                canvas.UndoStack.Push(line);

                canvas.SelectElementAt(new Point(50, 50));
                Assert.NotNull(canvas.SelectedElement);

                // Act: Delete
                var deleted = canvas.DeleteSelectedElement();
                Assert.True(deleted);
                Assert.Null(canvas.SelectedElement);
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
}
