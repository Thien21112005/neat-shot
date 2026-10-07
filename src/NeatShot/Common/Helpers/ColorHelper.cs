using System.Globalization;
using System.Windows.Media;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Tiện ích chuyển đổi mã màu giữa HEX và WPF Color/Brush.
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// Chuyển đổi Color sang chuỗi HEX chuẩn (định dạng #AARRGGBB).
    /// </summary>
    public static string ToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Phân tích chuỗi HEX (#RRGGBB hoặc #AARRGGBB) thành WPF Color.
    /// Trả về màu mặc định nếu chuỗi không hợp lệ.
    /// </summary>
    public static Color FromHex(string hex, Color? fallback = null)
    {
        var defaultColor = fallback ?? Colors.Black;

        if (string.IsNullOrWhiteSpace(hex))
        {
            return defaultColor;
        }

        var cleanHex = hex.Trim().TrimStart('#');

        // Định dạng #RRGGBB (6 ký tự)
        if (cleanHex.Length == 6)
        {
            if (byte.TryParse(cleanHex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                byte.TryParse(cleanHex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                byte.TryParse(cleanHex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                return Color.FromArgb(255, r, g, b);
            }
        }
        // Định dạng #AARRGGBB (8 ký tự)
        else if (cleanHex.Length == 8)
        {
            if (byte.TryParse(cleanHex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) &&
                byte.TryParse(cleanHex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) &&
                byte.TryParse(cleanHex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) &&
                byte.TryParse(cleanHex[6..8], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            {
                return Color.FromArgb(a, r, g, b);
            }
        }

        return defaultColor;
    }
}
