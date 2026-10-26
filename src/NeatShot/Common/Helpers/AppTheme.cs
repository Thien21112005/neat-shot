using Microsoft.Win32;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Chọn giao diện sáng/tối: theo lựa chọn của người dùng, hoặc theo Windows khi để "system".
/// </summary>
public static class AppTheme
{
    public const string System = "system";
    public const string Light = "light";
    public const string Dark = "dark";

    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>
    /// true nếu nên dùng giao diện sáng.
    /// </summary>
    public static bool IsLight(string? choice, bool windowsUsesLightTheme) => choice switch
    {
        Light => true,
        Dark => false,
        System or null => windowsUsesLightTheme,
        _ => false
    };

    /// <summary>
    /// Đọc chế độ ứng dụng của Windows (Settings → Personalization → Colors → "Choose your app mode").
    /// </summary>
    public static bool WindowsUsesLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            return key?.GetValue("AppsUseLightTheme") is int value && value != 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
