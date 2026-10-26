using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Views;

public class ToastWindowTests
{
    [Theory]
    [InlineData(@"C:\Users\me\Pictures\NeatShot\NeatShot_20261026_101500.png", @"Đã lưu vào Pictures\NeatShot")]
    [InlineData(@"D:\Shots\NeatShot_20261026_101500.png", @"Đã lưu vào D:\Shots")]
    public void DescribeSavedLocation_ShowsTheLastTwoFolderSegments(string savedPath, string expected)
    {
        Assert.Equal(expected, ToastWindow.DescribeSavedLocation(savedPath));
    }

    [Fact]
    public void DescribeSavedLocation_ReturnsNull_WhenNothingWasSaved()
    {
        Assert.Null(ToastWindow.DescribeSavedLocation(null));
        Assert.Null(ToastWindow.DescribeSavedLocation(""));
    }

    [Fact]
    public void ToastWindow_ShowsMessageAndDetail_WithoutStealingFocus_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var toast = new ToastWindow("Đã sao chép vào Clipboard", @"Đã lưu vào Pictures\NeatShot");

                Assert.Equal("Đã sao chép vào Clipboard", toast.MessageText.Text);
                Assert.Equal(@"Đã lưu vào Pictures\NeatShot", toast.DetailText.Text);
                Assert.Equal(Visibility.Visible, toast.DetailText.Visibility);
                Assert.False(toast.ShowActivated);
                Assert.True(toast.Topmost);
                Assert.False(toast.ShowInTaskbar);

                var noDetail = new ToastWindow("Đã sao chép vào Clipboard", null);
                Assert.Equal(Visibility.Collapsed, noDetail.DetailText.Visibility);
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
