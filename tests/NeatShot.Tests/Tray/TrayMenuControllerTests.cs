using NeatShot.Presentation.Tray;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;

namespace NeatShot.Tests.Tray;

public class TrayMenuControllerTests
{
    private const uint WM_ACTIVATE = 0x0006;
    private const int WA_INACTIVE = 0;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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

    private static void Flush() =>
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

    [Fact]
    public void Open_ShowsTheMenu_InItsOwnWindow()
    {
        RunOnSta(() =>
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Chụp vùng chọn" });
            var controller = new TrayMenuController(menu);

            controller.Open();
            Flush();

            Assert.True(menu.IsOpen);
            Assert.NotEqual(IntPtr.Zero, controller.PopupHandle);

            menu.IsOpen = false;
        });
    }

    [Fact]
    public void Menu_Closes_WhenItsWindowLosesActivation()
    {
        // Click ra ngoài (hoặc Alt+Tab) làm cửa sổ menu mất kích hoạt -> menu phải tự đóng
        RunOnSta(() =>
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Thoát" });
            var controller = new TrayMenuController(menu);
            controller.Open();
            Flush();

            SendMessage(controller.PopupHandle, WM_ACTIVATE, new IntPtr(WA_INACTIVE), IntPtr.Zero);
            Flush();

            Assert.False(menu.IsOpen);
        });
    }
}
