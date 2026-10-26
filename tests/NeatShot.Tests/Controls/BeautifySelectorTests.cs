using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xunit;

namespace NeatShot.Tests.Controls;

public class BeautifySelectorTests
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

    private static void Flush() =>
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);

    private static T? FindChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) return match;
            var nested = FindChild<T>(child);
            if (nested != null) return nested;
        }
        return null;
    }

    [Fact]
    public void BeautifySelector_TemplateExposesPartPopup_SoComboBoxCanManageItsDropDown()
    {
        RunOnSta(() =>
        {
            var toolbar = new AnnotationToolbar();
            toolbar.BeautifyComboBox.ApplyTemplate();

            var popup = toolbar.BeautifyComboBox.Template.FindName("PART_Popup", toolbar.BeautifyComboBox);
            Assert.IsType<Popup>(popup);
        });
    }

    [Fact]
    public void BeautifySelector_ShowsTheSelectedPresetGradient_AndNoneWhenOriginal()
    {
        RunOnSta(() =>
        {
            var toolbar = new AnnotationToolbar();
            Assert.Null(toolbar.BeautifyComboBox.Tag);

            toolbar.SelectBeautifyPreset(BeautifyPreset.Ocean);
            var swatch = Assert.IsType<LinearGradientBrush>(toolbar.BeautifyComboBox.Tag);
            Assert.Equal(Color.FromRgb(0x00, 0x93, 0xE9), swatch.GradientStops[0].Color);

            toolbar.SelectBeautifyPreset(BeautifyPreset.None);
            Assert.Null(toolbar.BeautifyComboBox.Tag);
        });
    }

    [Fact]
    public void BeautifySelector_ClickOpensDropDown_AndPickingAPresetAppliesIt()
    {
        RunOnSta(() =>
        {
            var toolbar = new AnnotationToolbar();
            // Nạp theme WPF-UI giống App.xaml để tái hiện đúng môi trường của app thật
            var host = new Grid();
            host.Resources.MergedDictionaries.Add(new Wpf.Ui.Markup.ThemesDictionary { Theme = Wpf.Ui.Appearance.ApplicationTheme.Dark });
            host.Resources.MergedDictionaries.Add(new Wpf.Ui.Markup.ControlsDictionary());
            host.Children.Add(toolbar);
            var window = new Window
            {
                Content = host,
                Width = 900,
                Height = 600,
                Left = -20000,
                Top = -20000,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };
            BeautifyOptions? applied = null;
            toolbar.BeautifyChanged += (s, options) => applied = options;

            try
            {
                window.Show();
                toolbar.UpdatePositions(new Rect(80, 40, 500, 300), new Size(900, 600));
                Flush();

                var combo = toolbar.BeautifyComboBox;
                Assert.True(combo.IsEnabled);

                // Bấm vào ô chọn (nút gạt nằm trong template) — vùng bấm phải phủ kín cả ô
                var toggle = FindChild<ToggleButton>(combo);
                Assert.NotNull(toggle);
                Assert.True(toggle!.ActualWidth >= combo.ActualWidth - 1,
                    $"Toggle width {toggle.ActualWidth} must cover the selector width {combo.ActualWidth}");
                toggle.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
                Flush();
                Assert.True(combo.IsDropDownOpen);

                // Chọn preset "Ocean" trong danh sách xổ xuống
                var ocean = (ComboBoxItem)combo.Items[2];
                ocean.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonUpEvent });
                Flush();

                Assert.Equal(2, combo.SelectedIndex);
                Assert.False(combo.IsDropDownOpen);
                Assert.Equal(BeautifyPreset.Ocean, applied?.Preset);
            }
            finally
            {
                window.Close();
            }
        });
    }
}
