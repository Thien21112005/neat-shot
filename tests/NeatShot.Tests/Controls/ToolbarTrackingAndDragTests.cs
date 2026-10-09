using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.Controls;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Controls;

public class ToolbarTrackingAndDragTests
{
    [Fact]
    public void AnnotationToolbar_DragGrip_ClickWithoutThreshold_DoesNotTriggerToolbarMoved_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var moveCallCount = 0;
                toolbar.ToolbarMoved += (s, pt) => moveCallCount++;

                // 1. Simulate mouse down on DragGrip
                var downArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent
                };
                toolbar.DragGrip.RaiseEvent(downArgs);

                // 2. Simulate micro jitter mouse move (only 1px movement)
                var moveArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = UIElement.MouseMoveEvent
                };
                toolbar.DragGrip.RaiseEvent(moveArgs);

                // 3. Mouse up
                var upArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonUpEvent
                };
                toolbar.DragGrip.RaiseEvent(upArgs);

                // Assert: Micro move should NOT trigger ToolbarMoved
                Assert.Equal(0, moveCallCount);
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
    public void OverlayWindow_MovingCropRegion_ReAnchorsToolbar_EvenIfManuallyMoved_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                // 1. Initial crop selection
                vm.SelectedRegion = new CaptureRegion(100, 100, 200, 200);

                var initialLeft = Canvas.GetLeft(window.Toolbar.VerticalBar);
                Assert.True(initialLeft >= 300, $"Initial toolbar position should be >= 300, was {initialLeft}");

                // 2. Simulate user manually moving the toolbar (firing ToolbarMoved)
                var field = typeof(AnnotationToolbar).GetField("ToolbarMoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                var manualPoint = new Point(50, 50);
                var eventDelegate = field?.GetValue(window.Toolbar) as MulticastDelegate;
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object?[] { window.Toolbar, manualPoint });
                    }
                }

                // 3. User moves crop frame far to the right (x=700, y=100)
                vm.SelectedRegion = new CaptureRegion(700, 100, 200, 200);

                // 4. Assert: Toolbar MUST track the new crop frame position (> 900)
                var updatedLeft = Canvas.GetLeft(window.Toolbar.VerticalBar);
                Assert.True(updatedLeft >= 900, $"Toolbar must follow the crop box when crop box moves! Expected >= 900, but was {updatedLeft}");
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
    public void OverlayWindow_ResetPosition_ReAnchorsToolbar_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new OverlayViewModel();
                var captureService = new ScreenCaptureService();
                var exportService = new ExportService(captureService);
                var window = new OverlayWindow(vm, exportService);

                vm.SelectedRegion = new CaptureRegion(200, 200, 300, 300);
                var defaultLeft = Canvas.GetLeft(window.Toolbar.VerticalBar);

                // Move manually
                window.Toolbar.MoveVerticalBarDelta(100, 100);
                Assert.Equal(defaultLeft + 100, Canvas.GetLeft(window.Toolbar.VerticalBar));

                // Trigger reset position
                var resetField = typeof(AnnotationToolbar).GetField("ToolbarResetPosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                var resetDelegate = resetField?.GetValue(window.Toolbar) as MulticastDelegate;
                if (resetDelegate != null)
                {
                    foreach (var handler in resetDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object?[] { window.Toolbar, EventArgs.Empty });
                    }
                }

                // Assert: Re-anchored to default position
                Assert.Equal(defaultLeft, Canvas.GetLeft(window.Toolbar.VerticalBar));
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
