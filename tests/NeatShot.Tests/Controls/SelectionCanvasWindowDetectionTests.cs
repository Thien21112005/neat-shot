using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Controls;

public class SelectionCanvasWindowDetectionTests
{
    private static readonly Rect WindowUnderCursor = new(100, 80, 300, 200);

    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { threadException = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(threadException);
    }

    private static SelectionCanvas CreateCanvas()
    {
        var canvas = new SelectionCanvas
        {
            HoverRegionProvider = p => WindowUnderCursor.Contains(p) ? WindowUnderCursor : null
        };
        canvas.Measure(new Size(800, 600));
        canvas.Arrange(new Rect(0, 0, 800, 600));
        return canvas;
    }

    [Fact]
    public void HoveringAWindow_OutlinesIt_WithoutHandles_OnStaThread()
    {
        RunOnSta(() =>
        {
            var canvas = CreateCanvas();

            canvas.HandlePointerMove(new Point(200, 150));

            Assert.Equal(WindowUnderCursor, canvas.HoveredRegion);
            Assert.Equal(Visibility.Visible, canvas.SelectionBorder.Visibility);
            Assert.Equal(Visibility.Collapsed, canvas.HandlesLayer.Visibility);
            Assert.False(canvas.SelectedRegion.IsValid);

            canvas.HandlePointerMove(new Point(700, 500));
            Assert.Null(canvas.HoveredRegion);
            Assert.Equal(Visibility.Collapsed, canvas.SelectionBorder.Visibility);
        });
    }

    [Fact]
    public void ClickingWithoutDragging_SelectsTheHoveredWindow_OnStaThread()
    {
        RunOnSta(() =>
        {
            var canvas = CreateCanvas();
            var completed = 0;
            canvas.SelectionCompleted += (s, e) => completed++;

            canvas.HandlePointerMove(new Point(200, 150));
            canvas.HandlePointerDown(new Point(200, 150));
            canvas.HandlePointerUp(new Point(201, 151));

            Assert.Equal(new CaptureRegion(100, 80, 300, 200), canvas.SelectedRegion);
            Assert.Equal(Visibility.Visible, canvas.HandlesLayer.Visibility);
            Assert.Equal(1, completed);
        });
    }

    [Fact]
    public void Dragging_StillSelectsAFreeRegion_EvenOverAWindow_OnStaThread()
    {
        RunOnSta(() =>
        {
            var canvas = CreateCanvas();

            canvas.HandlePointerMove(new Point(150, 100));
            canvas.HandlePointerDown(new Point(150, 100));
            canvas.HandlePointerMove(new Point(250, 180));
            canvas.HandlePointerUp(new Point(250, 180));

            Assert.Equal(new CaptureRegion(150, 100, 100, 80), canvas.SelectedRegion);
        });
    }

    [Fact]
    public void HoverIsIgnored_OnceARegionIsSelected_OnStaThread()
    {
        RunOnSta(() =>
        {
            var canvas = CreateCanvas();
            canvas.SelectedRegion = new CaptureRegion(500, 400, 100, 50);

            canvas.HandlePointerMove(new Point(200, 150));

            Assert.Null(canvas.HoveredRegion);
        });
    }

    [Fact]
    public void HoveredWindow_IsClampedToTheScreen_OnStaThread()
    {
        RunOnSta(() =>
        {
            var canvas = new SelectionCanvas { HoverRegionProvider = _ => new Rect(-50, -20, 300, 200) };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));

            canvas.HandlePointerMove(new Point(10, 10));

            Assert.Equal(new Rect(0, 0, 250, 180), canvas.HoveredRegion);
        });
    }
}
