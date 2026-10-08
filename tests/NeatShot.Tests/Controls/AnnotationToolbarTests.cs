using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Controls;

public class AnnotationToolbarTests
{
    [Fact]
    public void AnnotationToolbar_ToolToggle_FiresEventsCorrectly_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var selectedTools = new List<DrawingToolType>();
                toolbar.ToolSelected += (s, tool) => selectedTools.Add(tool);

                // Act: Click Pencil (first click selects)
                toolbar.PencilButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Single(selectedTools);
                Assert.Equal(DrawingToolType.Pencil, selectedTools.Last());

                // Click Pencil again (second click deselects)
                toolbar.PencilButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(2, selectedTools.Count);
                Assert.Equal(DrawingToolType.None, selectedTools.Last());

                // Click Rectangle
                toolbar.RectButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(3, selectedTools.Count);
                Assert.Equal(DrawingToolType.Rectangle, selectedTools.Last());

                // ResetTools
                toolbar.ResetTools();
                Assert.Equal(4, selectedTools.Count);
                Assert.Equal(DrawingToolType.None, selectedTools.Last());
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
    public void AnnotationToolbar_ColorSelect_FiresColorEvent_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                Color? selectedColor = null;
                toolbar.ColorSelected += (s, color) => selectedColor = color;

                toolbar.ColorBlueButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                Assert.NotNull(selectedColor);
                Assert.Equal(Color.FromRgb(0x00, 0x78, 0xD4), selectedColor.Value);
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
    public void AnnotationToolbar_ColorHighlight_OnlyWhenToolActive_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();

                // 1. Initial state (Tool is None) -> No color has white border
                Assert.Equal(Brushes.Transparent, toolbar.ColorRedButton.BorderBrush);
                Assert.Equal(Brushes.Transparent, toolbar.ColorBlueButton.BorderBrush);

                // 2. Activate Pencil -> Red button (default) gets White border
                toolbar.PencilButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(Brushes.White, toolbar.ColorRedButton.BorderBrush);
                Assert.Equal(Brushes.Transparent, toolbar.ColorBlueButton.BorderBrush);

                // 3. Select Blue -> Blue button gets White border, Red loses it
                toolbar.ColorBlueButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(Brushes.Transparent, toolbar.ColorRedButton.BorderBrush);
                Assert.Equal(Brushes.White, toolbar.ColorBlueButton.BorderBrush);

                // 4. Toggle Pencil off -> No color button has White border
                toolbar.PencilButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(Brushes.Transparent, toolbar.ColorRedButton.BorderBrush);
                Assert.Equal(Brushes.Transparent, toolbar.ColorBlueButton.BorderBrush);
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
