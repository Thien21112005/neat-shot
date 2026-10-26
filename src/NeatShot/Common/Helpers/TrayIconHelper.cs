using System.Drawing;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Tạo icon cho khay hệ thống (System Tray) từ logo NeatShot nhúng trong assembly (Assets/NeatShot.ico),
/// cùng một icon với file exe, gói MSIX và trang Store.
/// </summary>
public static class TrayIconHelper
{
    private const string IconResourceName = "NeatShot.ico";
    private const int TrayIconSize = 32;

    public static Icon CreateTrayIcon()
    {
        using var stream = typeof(TrayIconHelper).Assembly.GetManifestResourceStream(IconResourceName)
            ?? throw new InvalidOperationException($"Không tìm thấy resource {IconResourceName}.");

        return new Icon(stream, TrayIconSize, TrayIconSize);
    }
}
