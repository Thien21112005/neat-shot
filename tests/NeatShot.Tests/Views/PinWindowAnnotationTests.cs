using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Views;

public class PinWindowAnnotationTests
{
    [Fact]
    public void PinWindow_HasDrawingCanvasAndAnnotationToolbar_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new PinViewModel();
                var window = new PinWindow(vm);
                var dummyBitmap = new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32);
                window.SetImage(dummyBitmap, new Point(50, 60));

                // Phải có lớp vẽ DrawingCanvas
                Assert.NotNull(window.DrawingControl);

                // Phải có thanh công cụ chú thích PinAnnotationToolbar
                Assert.NotNull(window.AnnotationToolbar);
                Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);

                // Có nút bật/tắt vẽ trên header
                Assert.NotNull(window.PinAnnotateButton);

                window.Close();
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
    public void PinWindow_PinAnnotateButton_TogglesAnnotationToolbar_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new PinViewModel();
                var window = new PinWindow(vm);
                var dummyBitmap = new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32);
                window.SetImage(dummyBitmap, new Point(50, 60));

                // Bấm nút bút vẽ trên header -> Thanh công cụ hiện lên
                window.PinAnnotateButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(Visibility.Visible, window.AnnotationToolbar.Visibility);

                // Bấm lại -> Thanh công cụ ẩn đi
                window.PinAnnotateButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);

                window.Close();
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
    public void PinWindow_SelectingDrawingTool_UpdatesDrawingControlTool_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new PinViewModel();
                var window = new PinWindow(vm);
                var dummyBitmap = new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32);
                window.SetImage(dummyBitmap, new Point(50, 60));

                // Chọn công cụ Rectangle từ AnnotationToolbar
                window.AnnotationToolbar.SelectTool(DrawingToolType.Rectangle);
                Assert.Equal(DrawingToolType.Rectangle, window.DrawingControl.CurrentTool);

                // Đổi sang Pencil
                window.AnnotationToolbar.SelectTool(DrawingToolType.Pencil);
                Assert.Equal(DrawingToolType.Pencil, window.DrawingControl.CurrentTool);

                window.Close();
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
    public void PinWindow_GetAnnotatedImage_IncludesDrawnAnnotations_OnStaThread()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var vm = new PinViewModel();
                var window = new PinWindow(vm);
                var dummyBitmap = new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32);
                window.SetImage(dummyBitmap, new Point(50, 60));

                // Thêm một nét vẽ hình chữ nhật vào UndoStack
                window.DrawingControl.UndoStack.Push(new DrawingElement
                {
                    ToolType = DrawingToolType.Rectangle,
                    Rect = new Rect(10, 10, 50, 40),
                    Color = Colors.Red,
                    Thickness = 2.0
                });

                var annotatedImage = window.GetAnnotatedImage();
                Assert.NotNull(annotatedImage);
                Assert.Equal(100, annotatedImage.PixelWidth);
                Assert.Equal(80, annotatedImage.PixelHeight);

                window.Close();
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
