using System.IO;
using System.Windows.Media.Imaging;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Chọn bộ mã hoá ảnh (PNG/JPG) theo phần mở rộng file hoặc theo định dạng trong cài đặt.
/// </summary>
public static class ImageEncoderFactory
{
    public const int DefaultJpegQuality = 90;

    /// <summary>
    /// Phần mở rộng tương ứng với giá trị ImageFormat trong settings ("png" | "jpg"); mặc định ".png".
    /// </summary>
    public static string ExtensionFor(string? imageFormat) =>
        imageFormat?.Trim().ToLowerInvariant() is "jpg" or "jpeg" ? ".jpg" : ".png";

    /// <summary>
    /// Tạo encoder theo phần mở rộng của đường dẫn: .jpg/.jpeg → JPEG (kèm chất lượng), còn lại → PNG.
    /// </summary>
    public static BitmapEncoder CreateForPath(string path, int jpegQuality = DefaultJpegQuality)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension is ".jpg" or ".jpeg")
        {
            return new JpegBitmapEncoder { QualityLevel = Math.Clamp(jpegQuality, 1, 100) };
        }

        return new PngBitmapEncoder();
    }
}
