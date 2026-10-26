using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace NeatShot.Tests.Controls;

public class PinAnnotationToolbarTests
{
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

    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

    /// <summary>Mọi nút công cụ / thao tác (không tính ô màu).</summary>
    private static IEnumerable<(Button Button, DrawingToolType? Tool)> ToolButtons(PinAnnotationToolbar t) =>
    [
        (t.SelectButton, DrawingToolType.Select),
        (t.PencilButton, DrawingToolType.Pencil),
        (t.HighlightButton, DrawingToolType.Highlight),
        (t.RectButton, DrawingToolType.Rectangle),
        (t.EllipseButton, DrawingToolType.Ellipse),
        (t.LineButton, DrawingToolType.Line),
        (t.ArrowButton, DrawingToolType.Arrow),
        (t.TextButton, DrawingToolType.Text),
        (t.StepButton, DrawingToolType.StepCounter),
        (t.PixelateButton, DrawingToolType.Pixelate),
        (t.BlurButton, DrawingToolType.Blur),
        (t.UndoButton, null),
        (t.RedoButton, null),
        (t.HideToolbarButton, null),
    ];

    [Fact]
    public void EveryButton_UsesAVectorIcon_LikeTheMainToolbar()
    {
        RunOnSta(() =>
        {
            var toolbar = new PinAnnotationToolbar();

            foreach (var (button, _) in ToolButtons(toolbar))
            {
                var box = Assert.IsType<Viewbox>(button.Content);
                Assert.IsType<Path>(box.Child);
                Assert.False(string.IsNullOrWhiteSpace(button.ToolTip?.ToString()), $"{button.Name} cần tooltip");
            }
        });
    }

    [Fact]
    public void ClickingATool_SelectsIt_AndHighlightsOnlyThatButton()
    {
        RunOnSta(() =>
        {
            var toolbar = new PinAnnotationToolbar();
            var selected = new List<DrawingToolType>();
            toolbar.ToolSelected += (s, tool) => selected.Add(tool);

            foreach (var (button, tool) in ToolButtons(toolbar).Where(b => b.Tool != null))
            {
                Click(button);

                Assert.Equal(tool, toolbar.CurrentTool);
                Assert.Equal(tool, selected[^1]);
                foreach (var (other, otherTool) in ToolButtons(toolbar).Where(b => b.Tool != null))
                {
                    var isAccent = other.Background is SolidColorBrush { Color: var c } && c == Color.FromRgb(0, 120, 212);
                    Assert.Equal(otherTool == tool, isAccent);
                }
            }
        });
    }

    [Fact]
    public void UndoAndRedoButtons_RaiseTheirEvents()
    {
        RunOnSta(() =>
        {
            var toolbar = new PinAnnotationToolbar();
            var undo = 0;
            var redo = 0;
            toolbar.UndoRequested += (s, e) => undo++;
            toolbar.RedoRequested += (s, e) => redo++;

            Click(toolbar.UndoButton);
            Click(toolbar.RedoButton);

            Assert.Equal(1, undo);
            Assert.Equal(1, redo);
        });
    }

    [Fact]
    public void ChoosingAColor_MarksThatSwatchAsCurrent()
    {
        RunOnSta(() =>
        {
            var toolbar = new PinAnnotationToolbar();

            Click(toolbar.ColorBlueButton);

            Assert.Equal(Color.FromRgb(0, 120, 212), toolbar.CurrentColor);
            Assert.True(PinAnnotationToolbar.GetIsCurrent(toolbar.ColorBlueButton));
            Assert.False(PinAnnotationToolbar.GetIsCurrent(toolbar.ColorRedButton));
        });
    }
}
