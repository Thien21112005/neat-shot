using NeatShot.Common.Helpers;
using NeatShot.Core.Interop;
using NeatShot.Presentation.Tray;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;

namespace NeatShot.Tests.Tray;

public class NativeTrayIconTests
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int TrayIconId = 1001;

    private static bool TrayIconExists(IntPtr hwnd)
    {
        var data = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hwnd,
            uID = TrayIconId,
            uFlags = NativeConstants.NIF_TIP,
            szTip = "NeatShot test"
        };
        // NIM_MODIFY chỉ thành công khi icon đang tồn tại trên khay hệ thống
        return NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_MODIFY, ref data);
    }

    private static void RemoveTrayIconExternally(IntPtr hwnd)
    {
        var data = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hwnd,
            uID = TrayIconId
        };
        NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_DELETE, ref data);
    }

    [Fact]
    public void NativeTrayIcon_ReAddsIcon_WhenExplorerBroadcastsTaskbarCreated_OnStaThread()
    {
        using var icon = TrayIconHelper.CreateTrayIcon();

        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var tray = new NativeTrayIcon(icon.Handle, "NeatShot test");
                Assert.True(TrayIconExists(tray.Handle));

                // Mô phỏng Explorer khởi động lại: icon bị mất khỏi khay rồi Shell phát TaskbarCreated
                RemoveTrayIconExternally(tray.Handle);
                Assert.False(TrayIconExists(tray.Handle));

                SendMessage(tray.Handle, RegisterWindowMessage("TaskbarCreated"), IntPtr.Zero, IntPtr.Zero);

                Assert.True(TrayIconExists(tray.Handle));
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
