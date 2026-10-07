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
}
