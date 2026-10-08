namespace NeatShot.Core.Interop;

/// <summary>
/// Các hằng số Win32 API phục vụ chụp màn hình và đăng ký phím tắt.
/// </summary>
public static class NativeConstants
{
    // Raster Operation codes for BitBlt
    public const int SRCCOPY = 0x00CC0020;
    public const int CAPTUREBLT = 0x40000000;

    // ShowWindow Commands
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;

    // Window Messages
    public const int WM_HOTKEY = 0x0312;

    // Hotkey Modifiers for RegisterHotKey
    public const uint MOD_NONE = 0x0000;
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    // Console Attach Constants
    public const int ATTACH_PARENT_PROCESS = -1;

    // Shell_NotifyIcon Constants
    public const int NIM_ADD = 0x00000000;
    public const int NIM_MODIFY = 0x00000001;
    public const int NIM_DELETE = 0x00000002;
    public const int NIF_MESSAGE = 0x00000001;
    public const int NIF_ICON = 0x00000002;
    public const int NIF_TIP = 0x00000004;

    // Window Messages for Tray
    public const int WM_USER = 0x0400;
    public const int WM_TRAYICON = WM_USER + 101;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_RBUTTONUP = 0x0205;

    // System Metrics & Device Caps
    public const int SM_XVIRTUALSCREEN = 76;
    public const int SM_YVIRTUALSCREEN = 77;
    public const int SM_CXVIRTUALSCREEN = 78;
    public const int SM_CYVIRTUALSCREEN = 79;
    public const int DESKTOPHORZRES = 118;
    public const int DESKTOPVERTRES = 117;

    // Keyboard & Window Messages for Dismissing Popups
    public const byte VK_ESCAPE = 0x1B;
    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;
    public const int VK_MENU = 0x12; // Alt
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const int WM_CLOSE = 0x0010;
}
