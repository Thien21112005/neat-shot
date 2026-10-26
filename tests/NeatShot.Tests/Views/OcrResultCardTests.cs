using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace NeatShot.Tests.Views;

public class OcrResultCardTests
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

    private static IEnumerable<DependencyObject> LogicalDescendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;
            foreach (var nested in LogicalDescendants(child)) yield return nested;
        }
    }

    private static bool ContainsEmoji(string? text) =>
        !string.IsNullOrEmpty(text) && text.EnumerateRunes().Any(r => r.Value >= 0x1F300 || (r.Value >= 0x2600 && r.Value <= 0x27BF));

    [Fact]
    public void OcrCard_UsesVectorIcons_NotEmoji_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new OverlayWindow(new OverlayViewModel(), new ExportService(new ScreenCaptureService()));
            var nodes = LogicalDescendants(window.OcrResultCard).ToList();

            Assert.DoesNotContain(nodes.OfType<TextBlock>(), t => ContainsEmoji(t.Text));
            Assert.DoesNotContain(nodes.OfType<ContentControl>(), c => ContainsEmoji(c.Content as string));
            Assert.True(nodes.OfType<Path>().Count() >= 4, "header, status and action buttons should use vector icons");
        });
    }

    [Fact]
    public void ShowOcrResult_FillsTextAndSummarizesLinesAndCharacters_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new OverlayWindow(new OverlayViewModel(), new ExportService(new ScreenCaptureService()));

            window.ShowOcrResult("Xin chào\nNeatShot");

            Assert.Equal(Visibility.Visible, window.OcrResultCard.Visibility);
            Assert.Equal("Xin chào\nNeatShot", window.OcrResultTextBox.Text);
            Assert.Equal("2 dòng · 16 ký tự", window.OcrResultMetaText.Text);
        });
    }
}
