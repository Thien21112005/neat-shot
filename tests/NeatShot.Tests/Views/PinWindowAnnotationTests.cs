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

                // Mặc định ban đầu: Cả PinControlBar và PinAnnotationToolbar đều ẩn (Collapsed)
                Assert.NotNull(window.PinControlBar);
                Assert.Equal(Visibility.Collapsed, window.PinControlBar.Visibility);

                Assert.NotNull(window.AnnotationToolbar);
                Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);

                // Header không còn nút bút chì: click vào ảnh (hoặc Ctrl + P) đã bật/tắt chế độ vẽ
                Assert.Null(window.FindName("PinAnnotateButton"));
                Assert.NotNull(window.PinCopyButton);
                Assert.NotNull(window.PinCloseButton);

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
    public void PinWindow_CtrlP_TogglesAnnotationToolbar_OnStaThread()
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

                // Ctrl + P -> Thanh công cụ hiện lên
                window.ProcessShortcut(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Control);
                Assert.Equal(Visibility.Visible, window.AnnotationToolbar.Visibility);

                // Ctrl + P lần nữa -> Thanh công cụ ẩn đi
                window.ProcessShortcut(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Control);
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

    [Fact]
    public void PinWindow_ToggleAllControls_TogglesBothHeaderAndAnnotationToolbar_OnStaThread()
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

                // Ban đầu: Cả 2 đều ẩn
                Assert.Equal(Visibility.Collapsed, window.PinControlBar.Visibility);
                Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);

                // Nhấn vào ảnh lần 1 -> Cả 2 hiện lên
                window.ToggleAllControls();
                Assert.Equal(Visibility.Visible, window.PinControlBar.Visibility);
                Assert.Equal(Visibility.Visible, window.AnnotationToolbar.Visibility);
                Assert.Equal(DrawingToolType.None, window.DrawingControl.CurrentTool);

                // Nhấn vào ảnh lần 2 -> Cả 2 tắt đi
                window.ToggleAllControls();
                Assert.Equal(Visibility.Collapsed, window.PinControlBar.Visibility);
                Assert.Equal(Visibility.Collapsed, window.AnnotationToolbar.Visibility);
                Assert.Equal(DrawingToolType.None, window.DrawingControl.CurrentTool);

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

    [Fact]
    public void PinWindow_RedoButton_RestoresTheLastUndoneStroke_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new PinWindow(new PinViewModel());
            window.SetImage(new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32), new Point(50, 60));
            window.DrawingControl.UndoStack.Push(new DrawingElement { ToolType = DrawingToolType.Pencil });

            window.AnnotationToolbar.UndoButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Assert.Equal(0, window.DrawingControl.UndoStack.Count);

            window.AnnotationToolbar.RedoButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            Assert.Equal(1, window.DrawingControl.UndoStack.Count);
            window.Close();
        });
    }

    [Fact]
    public void PinWindow_ShowsTheToolbarBelowTheImage_WithoutWideningTheWindow_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new PinWindow(new PinViewModel()) { ShowActivated = false };
            window.SetImage(new RenderTargetBitmap(120, 80, 96, 96, PixelFormats.Pbgra32), new Point(-20000, 0));
            window.Show();
            window.UpdateLayout();
            var widthBefore = window.ActualWidth;

            // Thanh vẽ nằm trong Popup nổi riêng: ảnh nhỏ không bị nới rộng hay bị che
            Assert.IsType<System.Windows.Controls.Primitives.Popup>(window.AnnotationPopup);
            Assert.Same(window.AnnotationToolbar, window.AnnotationPopup.Child);

            window.ToggleAllControls(true);
            window.UpdateLayout();
            Assert.True(window.AnnotationPopup.IsOpen);
            Assert.Equal(widthBefore, window.ActualWidth, 1);

            window.ToggleAllControls(false);
            Assert.False(window.AnnotationPopup.IsOpen);
            window.Close();
        });
    }

    [Theory]
    [InlineData(System.Windows.Input.Key.V, DrawingToolType.Select)]
    [InlineData(System.Windows.Input.Key.H, DrawingToolType.Highlight)]
    [InlineData(System.Windows.Input.Key.R, DrawingToolType.Rectangle)]
    [InlineData(System.Windows.Input.Key.E, DrawingToolType.Ellipse)]
    [InlineData(System.Windows.Input.Key.L, DrawingToolType.Line)]
    [InlineData(System.Windows.Input.Key.A, DrawingToolType.Arrow)]
    [InlineData(System.Windows.Input.Key.T, DrawingToolType.Text)]
    [InlineData(System.Windows.Input.Key.N, DrawingToolType.StepCounter)]
    [InlineData(System.Windows.Input.Key.X, DrawingToolType.Pixelate)]
    [InlineData(System.Windows.Input.Key.B, DrawingToolType.Blur)]
    public void PinWindow_ToolLetters_PickTools_WhileTheToolbarIsOpen_OnStaThread(System.Windows.Input.Key key, DrawingToolType expected)
    {
        RunOnSta(() =>
        {
            var window = new PinWindow(new PinViewModel());
            window.SetImage(new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32), new Point(50, 60));

            // Thanh vẽ đang ẩn: phím chữ không làm gì
            Assert.False(window.ProcessShortcut(key, System.Windows.Input.ModifierKeys.None));

            window.ToggleAnnotationToolbar(true);
            Assert.True(window.ProcessShortcut(key, System.Windows.Input.ModifierKeys.None));
            Assert.Equal(expected, window.AnnotationToolbar.CurrentTool);
            window.Close();
        });
    }

    [Fact]
    public void PinWindow_CtrlZ_And_CtrlY_UndoAndRedo_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = new PinWindow(new PinViewModel());
            window.SetImage(new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32), new Point(50, 60));
            window.DrawingControl.UndoStack.Push(new DrawingElement { ToolType = DrawingToolType.Pencil });

            Assert.True(window.ProcessShortcut(System.Windows.Input.Key.Z, System.Windows.Input.ModifierKeys.Control));
            Assert.Equal(0, window.DrawingControl.UndoStack.Count);

            Assert.True(window.ProcessShortcut(System.Windows.Input.Key.Y, System.Windows.Input.ModifierKeys.Control));
            Assert.Equal(1, window.DrawingControl.UndoStack.Count);
            window.Close();
        });
    }
}
