using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Cung cấp tiện ích trích xuất mã màu pixel từ BitmapSource dựa trên toạ độ con trỏ (DIP) và tỉ lệ DPI scaling.
/// Dùng cho tính năng kính lúp và cây bút hút màu hệ thống (Eyedropper).
/// </summary>
public static class ColorPickerHelper
{
    /// <summary>
    /// Lấy màu Color (RGB) tại toạ độ DIP trên ảnh BitmapSource, tự động xử lý chuyển đổi tỉ lệ DPI.
    /// </summary>
    /// <param name="bitmap">Ảnh nguồn (ví dụ ảnh chụp màn hình đóng băng).</param>
    /// <param name="dipPoint">Toạ độ DIP (Device Independent Pixels) của con trỏ chuột.</param>
    /// <param name="dpiX">Chỉ số DPI ngang (mặc định 96.0).</param>
    /// <param name="dpiY">Chỉ số DPI dọc (mặc định 96.0).</param>
    /// <returns>Màu WPF Color tại pixel đó.</returns>
    public static Color GetPixelColor(BitmapSource bitmap, Point dipPoint, double dpiX = 96.0, double dpiY = 96.0)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        var effDpiX = dpiX > 0 ? dpiX : (bitmap.DpiX > 0 ? bitmap.DpiX : 96.0);
        var effDpiY = dpiY > 0 ? dpiY : (bitmap.DpiY > 0 ? bitmap.DpiY : 96.0);

        var scaleX = effDpiX / 96.0;
        var scaleY = effDpiY / 96.0;

        var px = (int)Math.Clamp(Math.Round(dipPoint.X * scaleX), 0, Math.Max(0, bitmap.PixelWidth - 1));
        var py = (int)Math.Clamp(Math.Round(dipPoint.Y * scaleY), 0, Math.Max(0, bitmap.PixelHeight - 1));

        BitmapSource source = bitmap;
        if (bitmap.Format != PixelFormats.Bgra32 && bitmap.Format != PixelFormats.Pbgra32)
        {
            source = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        }

        var pixel = new byte[4];
        source.CopyPixels(new Int32Rect(px, py, 1, 1), pixel, 4, 0);

        return Color.FromRgb(pixel[2], pixel[1], pixel[0]);
    }
}
