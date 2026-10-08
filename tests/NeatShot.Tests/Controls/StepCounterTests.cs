using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Controls;

public class StepCounterTests
{
    [Fact]
    public void DrawingCanvas_StepCounter_IncrementsAndSyncsWithUndoRedo_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas();
                canvas.CurrentTool = DrawingToolType.StepCounter;

                Assert.Equal(1, canvas.NextStepNumber);

                // Mô phỏng click tạo bước 1
                canvas.StartDrawing(new Point(50, 50));
                canvas.EndDrawing();

                Assert.Single(canvas.UndoStack.Items);
                var step1 = canvas.UndoStack.Items.First();
                Assert.Equal(DrawingToolType.StepCounter, step1.ToolType);
                Assert.Equal(1, step1.StepNumber);
                Assert.Equal(2, canvas.NextStepNumber);

                // Mô phỏng click tạo bước 2
                canvas.StartDrawing(new Point(100, 100));
                canvas.EndDrawing();

                Assert.Equal(2, canvas.UndoStack.Items.Count);
                var step2 = canvas.UndoStack.Items.Last();
                Assert.Equal(2, step2.StepNumber);
                Assert.Equal(3, canvas.NextStepNumber);

                // Undo: Bước 2 bị bỏ, số tiếp theo quay về 2
                canvas.Undo();
                Assert.Single(canvas.UndoStack.Items);
                Assert.Equal(2, canvas.NextStepNumber);

                // Redo: Bước 2 được phục hồi, số tiếp theo quay lại 3
                canvas.Redo();
                Assert.Equal(2, canvas.UndoStack.Items.Count);
                Assert.Equal(3, canvas.NextStepNumber);
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
    public void DrawingRenderer_RenderStepBadge_ProducesDrawing_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var visual = new DrawingVisual();
                var element = new DrawingElement
                {
                    ToolType = DrawingToolType.StepCounter,
                    StartPoint = new Point(40, 40),
                    StepNumber = 1,
                    Color = Colors.Blue
                };

                using (var dc = visual.RenderOpen())
                {
                    DrawingRenderer.RenderElement(dc, element);
                }

                Assert.NotNull(visual.Drawing);
                Assert.True(visual.Drawing.Children.Count > 0, "StepCounter must produce drawing content");
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
    public void HitTestHelper_StepCounter_DetectsHitWithinRadius()
    {
        var element = new DrawingElement
        {
            ToolType = DrawingToolType.StepCounter,
            StartPoint = new Point(50, 50),
            StepNumber = 1
        };

        // Điểm nằm trong bán kính 14 px
        Assert.True(HitTestHelper.HitTest(element, new Point(55, 55)));

        // Điểm nằm xa ngoài bán kính
        Assert.False(HitTestHelper.HitTest(element, new Point(100, 100)));
    }

    [Fact]
    public void AnnotationToolbar_StepCounterButton_TogglesTool_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                Assert.NotNull(toolbar.StepCounterButton);
                toolbar.StepCounterButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.StepCounter, toolbar.ActiveTool);
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
