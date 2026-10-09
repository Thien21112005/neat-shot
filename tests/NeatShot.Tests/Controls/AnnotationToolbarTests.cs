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

    [Fact]
    public void AnnotationToolbar_NewTools_ToggleCorrectly_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var selectedTools = new List<DrawingToolType>();
                toolbar.ToolSelected += (s, tool) => selectedTools.Add(tool);

                // Ellipse
                toolbar.EllipseButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Ellipse, selectedTools.Last());

                // Line
                toolbar.LineButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Line, selectedTools.Last());

                // Highlight
                toolbar.HighlightButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Highlight, selectedTools.Last());

                // Text
                toolbar.TextButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Text, selectedTools.Last());

                // Select
                toolbar.SelectButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Select, selectedTools.Last());

                // Eyedropper
                toolbar.EyedropperButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(DrawingToolType.Eyedropper, selectedTools.Last());
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
    public void AnnotationToolbar_ColorHistory_AddsSwatchesAndSelects_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                Color? selectedColor = null;
                toolbar.ColorSelected += (s, c) => selectedColor = c;

                var customColor = Color.FromRgb(123, 45, 67);
                toolbar.AddColorToHistory(customColor);

                Assert.Single(toolbar.History.Colors);
                Assert.Equal(customColor, toolbar.History.Colors[0]);
                Assert.Single(toolbar.HistoryColorsPanel.Children);
                Assert.Equal(customColor, selectedColor);

                // Add another color
                var customColor2 = Color.FromRgb(200, 100, 50);
                toolbar.AddColorToHistory(customColor2);
                Assert.Equal(2, toolbar.History.Colors.Count);
                Assert.Equal(2, toolbar.HistoryColorsPanel.Children.Count);

                // Click first swatch (which corresponds to customColor)
                var swatchButton = (System.Windows.Controls.Button)toolbar.HistoryColorsPanel.Children[1];
                swatchButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(customColor, selectedColor);
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
    public void AnnotationToolbar_ShapeDropdown_SelectsRectangleAndEllipse_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var selectedTools = new List<DrawingToolType>();
                toolbar.ToolSelected += (s, tool) => selectedTools.Add(tool);

                Assert.NotNull(toolbar.ShapeComboBox);
                Assert.True(toolbar.ShapeComboBox.Items.Count >= 2);

                // Select Rectangle (index 0)
                toolbar.SelectShapeTool(DrawingToolType.Rectangle);
                Assert.Equal(DrawingToolType.Rectangle, toolbar.ActiveTool);
                Assert.Equal(DrawingToolType.Rectangle, selectedTools.Last());

                // Select Ellipse (index 1)
                toolbar.SelectShapeTool(DrawingToolType.Ellipse);
                Assert.Equal(DrawingToolType.Ellipse, toolbar.ActiveTool);
                Assert.Equal(DrawingToolType.Ellipse, selectedTools.Last());
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
    public void AnnotationToolbar_LineDropdown_SelectsArrowAndLine_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var selectedTools = new List<DrawingToolType>();
                toolbar.ToolSelected += (s, tool) => selectedTools.Add(tool);

                Assert.NotNull(toolbar.LineComboBox);
                Assert.True(toolbar.LineComboBox.Items.Count >= 2);

                // Select Arrow
                toolbar.SelectLineTool(DrawingToolType.Arrow);
                Assert.Equal(DrawingToolType.Arrow, toolbar.ActiveTool);
                Assert.Equal(DrawingToolType.Arrow, selectedTools.Last());

                // Select Line
                toolbar.SelectLineTool(DrawingToolType.Line);
                Assert.Equal(DrawingToolType.Line, toolbar.ActiveTool);
                Assert.Equal(DrawingToolType.Line, selectedTools.Last());
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
    public void AnnotationToolbar_FontControls_ChangeFamilyAndSize_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                string? changedFamily = null;
                double? changedSize = null;

                toolbar.FontFamilyChanged += (s, f) => changedFamily = f;
                toolbar.FontSizeChanged += (s, size) => changedSize = size;

                Assert.NotNull(toolbar.FontFamilyComboBox);
                Assert.NotNull(toolbar.FontSizeComboBox);
                Assert.NotNull(toolbar.FontControlsPanel);

                // Initially with None tool, FontControlsPanel is Collapsed
                Assert.Equal(System.Windows.Visibility.Collapsed, toolbar.FontControlsPanel.Visibility);

                // Activate Text tool -> FontControlsPanel becomes Visible
                toolbar.SetActiveTool(DrawingToolType.Text);
                Assert.Equal(System.Windows.Visibility.Visible, toolbar.FontControlsPanel.Visibility);

                // Change font family
                toolbar.SelectFontFamily("Arial");
                Assert.Equal("Arial", changedFamily);
                Assert.Equal("Arial", toolbar.SelectedFontFamily);

                // Change font size
                toolbar.SelectFontSize(24.0);
                Assert.Equal(24.0, changedSize);
                Assert.Equal(24.0, toolbar.SelectedFontSize);

                // Activate Pencil -> FontControlsPanel is Collapsed
                toolbar.SetActiveTool(DrawingToolType.Pencil);
                Assert.Equal(System.Windows.Visibility.Collapsed, toolbar.FontControlsPanel.Visibility);
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
    public void AnnotationToolbar_LoadColorHistory_And_GetColorHistoryHex_PreservesColors_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var hexList = new List<string> { "#FFFF0000", "#FF00FF00", "#FF0000FF" };

                toolbar.LoadColorHistory(hexList);

                var retrievedHex = toolbar.GetColorHistoryHex();
                Assert.Equal(3, retrievedHex.Count);
                Assert.Equal("#FFFF0000", retrievedHex[0]);
                Assert.Equal("#FF00FF00", retrievedHex[1]);
                Assert.Equal("#FF0000FF", retrievedHex[2]);
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
    public void AnnotationToolbar_ShapeAndLineComboBox_ActivateImmediately_WithoutDropdownToggle_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var selectedTools = new List<DrawingToolType>();
                toolbar.ToolSelected += (s, tool) => selectedTools.Add(tool);

                // Initial state: tool is None
                Assert.Equal(DrawingToolType.None, toolbar.ActiveTool);

                // Simulate primary mouse down on ShapeComboBox (x=10, y=10)
                var mouseArgs = new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice,
                    0,
                    System.Windows.Input.MouseButton.Left)
                {
                    RoutedEvent = System.Windows.UIElement.PreviewMouseLeftButtonDownEvent
                };
                toolbar.ShapeComboBox.RaiseEvent(mouseArgs);

                // Should activate Rectangle immediately and keep dropdown closed
                Assert.Equal(DrawingToolType.Rectangle, toolbar.ActiveTool);
                Assert.False(toolbar.ShapeComboBox.IsDropDownOpen);
                Assert.Equal(DrawingToolType.Rectangle, selectedTools.Last());

                // Second click should toggle off to None
                var mouseArgs2 = new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice,
                    0,
                    System.Windows.Input.MouseButton.Left)
                {
                    RoutedEvent = System.Windows.UIElement.PreviewMouseLeftButtonDownEvent
                };
                toolbar.ShapeComboBox.RaiseEvent(mouseArgs2);
                Assert.Equal(DrawingToolType.None, toolbar.ActiveTool);
                Assert.Equal(DrawingToolType.None, selectedTools.Last());

                // Primary click on LineComboBox should activate Arrow immediately
                var mouseArgsLine = new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice,
                    0,
                    System.Windows.Input.MouseButton.Left)
                {
                    RoutedEvent = System.Windows.UIElement.PreviewMouseLeftButtonDownEvent
                };
                toolbar.LineComboBox.RaiseEvent(mouseArgsLine);
                Assert.Equal(DrawingToolType.Arrow, toolbar.ActiveTool);
                Assert.False(toolbar.LineComboBox.IsDropDownOpen);
                Assert.Equal(DrawingToolType.Arrow, selectedTools.Last());
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
    public void AnnotationToolbar_FlyoutPosition_AnchoredNearVerticalBar_NotAtZeroZero_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                var region = new System.Windows.Rect(200, 200, 400, 300);
                var screenSize = new System.Windows.Size(1920, 1080);

                // Position the toolbar
                toolbar.UpdatePositions(region, screenSize);

                // Act: Activate Text tool
                toolbar.SetActiveTool(DrawingToolType.Text);

                // Assert: FlyoutOptionsPanel must be visible
                Assert.Equal(System.Windows.Visibility.Visible, toolbar.FlyoutOptionsPanel.Visibility);

                // Position must NOT be at (0, 0) or NaN; must be near region or VerticalBar
                var flyoutX = System.Windows.Controls.Canvas.GetLeft(toolbar.FlyoutOptionsPanel);
                var flyoutY = System.Windows.Controls.Canvas.GetTop(toolbar.FlyoutOptionsPanel);

                Assert.False(double.IsNaN(flyoutX));
                Assert.False(double.IsNaN(flyoutY));
                Assert.True(flyoutX > 100, $"Expected flyoutX > 100, but was {flyoutX}");
                Assert.True(flyoutY > 100, $"Expected flyoutY > 100, but was {flyoutY}");
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
    public void AnnotationToolbar_DropdownItemClick_ActivatesTool_AndClosesDropdown_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                toolbar.ResetTools();
                Assert.Equal(DrawingToolType.None, toolbar.ActiveTool);

                // Open dropdown
                toolbar.ShapeComboBox.IsDropDownOpen = true;

                // Click first item (Rectangle, index 0, already selected item)
                var item0 = (System.Windows.Controls.ComboBoxItem)toolbar.ShapeComboBox.Items[0];
                var mouseArgs = new System.Windows.Input.MouseButtonEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice,
                    0,
                    System.Windows.Input.MouseButton.Left)
                {
                    RoutedEvent = System.Windows.UIElement.PreviewMouseLeftButtonUpEvent
                };
                item0.RaiseEvent(mouseArgs);

                // Assert tool is activated and dropdown is closed
                Assert.Equal(DrawingToolType.Rectangle, toolbar.ActiveTool);
                Assert.False(toolbar.ShapeComboBox.IsDropDownOpen);
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

