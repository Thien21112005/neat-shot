using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace NeatShot.Tests.Controls;

public class FontSizeAndStepSizeStepperTests
{
    [Fact]
    public void AnnotationToolbar_StepSizeStepperButtons_IncrementAndDecrementByOne_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                double receivedRadius = 0;
                toolbar.StepSizeChanged += (s, r) => receivedRadius = r;

                Assert.Equal(14.0, toolbar.SelectedStepRadius);

                // Act: Click increase button (+)
                toolbar.StepSizeIncreaseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(15.0, toolbar.SelectedStepRadius);
                Assert.Equal(15.0, receivedRadius);
                Assert.Equal("15", toolbar.StepSizeComboBox.Text);

                // Act: Click decrease button (-) twice
                toolbar.StepSizeDecreaseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(14.0, toolbar.SelectedStepRadius);
                Assert.Equal(14.0, receivedRadius);

                toolbar.StepSizeDecreaseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(13.0, toolbar.SelectedStepRadius);
                Assert.Equal(13.0, receivedRadius);
                Assert.Equal("13", toolbar.StepSizeComboBox.Text);
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
    public void AnnotationToolbar_FontSizeStepperButtons_IncrementAndDecrementByOne_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                double receivedSize = 0;
                toolbar.FontSizeChanged += (s, sz) => receivedSize = sz;

                Assert.Equal(16.0, toolbar.SelectedFontSize);

                // Act: Click increase button (+)
                toolbar.FontSizeIncreaseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(17.0, toolbar.SelectedFontSize);
                Assert.Equal(17.0, receivedSize);
                Assert.Equal("17", toolbar.FontSizeComboBox.Text);

                // Act: Click decrease button (-)
                toolbar.FontSizeDecreaseButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(16.0, toolbar.SelectedFontSize);
                Assert.Equal(16.0, receivedSize);
                Assert.Equal("16", toolbar.FontSizeComboBox.Text);
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
    public void AnnotationToolbar_DirectTextInput_UpdatesValuesAndFiresEvents_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                double receivedRadius = 0;
                double receivedFontSize = 0;
                toolbar.StepSizeChanged += (s, r) => receivedRadius = r;
                toolbar.FontSizeChanged += (s, sz) => receivedFontSize = sz;

                // StepSize direct text input
                toolbar.StepSizeComboBox.Text = "21";
                Assert.Equal(21.0, toolbar.SelectedStepRadius);
                Assert.Equal(21.0, receivedRadius);

                // FontSize direct text input
                toolbar.FontSizeComboBox.Text = "35";
                Assert.Equal(35.0, toolbar.SelectedFontSize);
                Assert.Equal(35.0, receivedFontSize);
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
    public void AnnotationToolbar_MouseWheel_IncrementsAndDecrements_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toolbar = new AnnotationToolbar();
                double receivedRadius = 0;
                double receivedFontSize = 0;
                toolbar.StepSizeChanged += (s, r) => receivedRadius = r;
                toolbar.FontSizeChanged += (s, sz) => receivedFontSize = sz;

                // Mouse wheel up on StepSizeComboBox (+120 delta)
                var wheelUpArgs = new System.Windows.Input.MouseWheelEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, 0, 120)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent
                };
                toolbar.StepSizeComboBox.RaiseEvent(wheelUpArgs);
                Assert.Equal(15.0, toolbar.SelectedStepRadius);
                Assert.Equal(15.0, receivedRadius);

                // Mouse wheel down on StepSizeComboBox (-120 delta)
                var wheelDownArgs = new System.Windows.Input.MouseWheelEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, 0, -120)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent
                };
                toolbar.StepSizeComboBox.RaiseEvent(wheelDownArgs);
                Assert.Equal(14.0, toolbar.SelectedStepRadius);
                Assert.Equal(14.0, receivedRadius);

                // Mouse wheel up on FontSizeComboBox (+120 delta)
                var fontWheelUpArgs = new System.Windows.Input.MouseWheelEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, 0, 120)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent
                };
                toolbar.FontSizeComboBox.RaiseEvent(fontWheelUpArgs);
                Assert.Equal(17.0, toolbar.SelectedFontSize);
                Assert.Equal(17.0, receivedFontSize);

                // Mouse wheel down on FontSizeComboBox (-120 delta)
                var fontWheelDownArgs = new System.Windows.Input.MouseWheelEventArgs(
                    System.Windows.Input.Mouse.PrimaryDevice, 0, -120)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent
                };
                toolbar.FontSizeComboBox.RaiseEvent(fontWheelDownArgs);
                Assert.Equal(16.0, toolbar.SelectedFontSize);
                Assert.Equal(16.0, receivedFontSize);
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
