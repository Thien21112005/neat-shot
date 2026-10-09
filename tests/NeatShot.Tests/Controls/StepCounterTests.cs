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
    public void DrawingCanvas_StepCounter_UsesCurrentColor_And_CustomRadius_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var canvas = new DrawingCanvas
                {
                    CurrentTool = DrawingToolType.StepCounter,
                    CurrentColor = Colors.Green,
                    CurrentStepRadius = 24.0
                };

                canvas.StartDrawing(new Point(100, 100));
                canvas.EndDrawing();

                var item = Assert.Single(canvas.UndoStack.Items);
                Assert.Equal(DrawingToolType.StepCounter, item.ToolType);
                Assert.Equal(Colors.Green, item.Color);
                Assert.Equal(24.0, item.Thickness);
                Assert.Equal(new Rect(76, 76, 48, 48), item.Rect);
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
    public void AnnotationToolbar_ColorPalette_OnlyVisibleWhenDrawingToolWithColorActive_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                // 1. Ban đầu khi chưa chọn công cụ vẽ (None / Kéo chọn vùng), bảng màu phải ẩn
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);

                // 2. Khi chọn Select, bảng màu vẫn ẩn
                toolbar.SetActiveTool(DrawingToolType.Select);
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);

                // 3. Khi chọn Pixelate, Blur, hoặc Eyedropper, bảng màu vẫn ẩn
                toolbar.SetActiveTool(DrawingToolType.Pixelate);
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);

                toolbar.SetActiveTool(DrawingToolType.Blur);
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);

                toolbar.SetActiveTool(DrawingToolType.Eyedropper);
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);

                // 4. Khi chọn Pencil, Rectangle, hoặc StepCounter, bảng màu mới hiển thị
                toolbar.SetActiveTool(DrawingToolType.Pencil);
                Assert.Equal(Visibility.Visible, toolbar.ColorPalettePanel.Visibility);

                toolbar.SetActiveTool(DrawingToolType.Rectangle);
                Assert.Equal(Visibility.Visible, toolbar.ColorPalettePanel.Visibility);

                toolbar.SetActiveTool(DrawingToolType.StepCounter);
                Assert.Equal(Visibility.Visible, toolbar.ColorPalettePanel.Visibility);

                // 5. Khi tắt công cụ (ResetTools / None), bảng màu ẩn lại
                toolbar.ResetTools();
                Assert.Equal(Visibility.Collapsed, toolbar.ColorPalettePanel.Visibility);
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
    public void AnnotationToolbar_VerticalBarButtons_HaveUniformWidth_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                Assert.Equal(32, toolbar.SelectButton.Width);
                Assert.Equal(32, toolbar.PencilButton.Width);
                Assert.Equal(32, toolbar.ShapeComboBox.Width);
                Assert.Equal(32, toolbar.LineComboBox.Width);
                Assert.Equal(32, toolbar.HighlightButton.Width);
                Assert.Equal(32, toolbar.TextButton.Width);
                Assert.Equal(32, toolbar.PixelateButton.Width);
                Assert.Equal(32, toolbar.BlurButton.Width);
                Assert.Equal(32, toolbar.StepCounterButton.Width);
                Assert.Equal(32, toolbar.EyedropperButton.Width);
                Assert.Equal(32, toolbar.UndoButton.Width);
                Assert.Equal(32, toolbar.RedoButton.Width);
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
    public void AnnotationToolbar_StepSizeSelection_FiresEvent_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                double receivedRadius = 0;
                toolbar.StepSizeChanged += (s, r) => receivedRadius = r;

                toolbar.SelectStepSize(18.0);
                Assert.Equal(18.0, toolbar.SelectedStepRadius);
                Assert.Equal(18.0, receivedRadius);
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
