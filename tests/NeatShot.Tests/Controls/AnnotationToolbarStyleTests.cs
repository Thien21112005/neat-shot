using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace NeatShot.Tests.Controls;

public class AnnotationToolbarStyleTests
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

    private static Path? FindIconPath(object? content)
    {
        return content switch
        {
            Path path => path,
            Viewbox { Child: var child } => FindIconPath(child),
            Panel panel => panel.Children.OfType<object>().Select(FindIconPath).FirstOrDefault(p => p != null),
            _ => null
        };
    }

    [Fact]
    public void ToolAndActionButtons_UseVectorLineIcons_InsteadOfEmoji()
    {
        RunOnSta(() =>
        {
            var toolbar = new AnnotationToolbar();
            var buttons = new Button[]
            {
                toolbar.SelectButton, toolbar.PencilButton, toolbar.HighlightButton, toolbar.TextButton,
                toolbar.PixelateButton, toolbar.BlurButton, toolbar.StepCounterButton, toolbar.EyedropperButton,
                toolbar.UndoButton, toolbar.RedoButton, toolbar.PinButton, toolbar.SaveButton, toolbar.CloseButton
            };

            foreach (var button in buttons)
            {
                var icon = FindIconPath(button.Content);
                Assert.True(icon != null, $"{button.Name} should render a vector icon");
                Assert.Null(icon!.Fill);
                Assert.True(icon.StrokeThickness > 0, $"{button.Name} icon should be a line (stroke) icon");
            }
        });
    }

    [Fact]
    public void ActiveTool_IsHighlightedWithSolidAccentAndWhiteForeground()
    {
        RunOnSta(() =>
        {
            var toolbar = new AnnotationToolbar();
            toolbar.SetActiveTool(DrawingToolType.Pencil);

            var background = Assert.IsType<SolidColorBrush>(toolbar.PencilButton.Background);
            Assert.Equal(Color.FromRgb(0x00, 0x78, 0xD4), background.Color);
            var foreground = Assert.IsType<SolidColorBrush>(toolbar.PencilButton.Foreground);
            Assert.Equal(Colors.White, foreground.Color);

            toolbar.SetActiveTool(DrawingToolType.Text);
            var inactive = toolbar.PencilButton.Background as SolidColorBrush;
            Assert.True(inactive == null || inactive.Color != Color.FromRgb(0x00, 0x78, 0xD4));
        });
    }
}
