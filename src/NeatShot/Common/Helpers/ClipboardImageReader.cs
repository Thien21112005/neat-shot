using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Đọc ảnh từ Clipboard để ghim lên màn hình. Thứ tự ưu tiên:
/// PNG (trình duyệt, Figma — giữ nền trong suốt) → Bitmap chuẩn (Paint, Zalo, Snipping Tool) → file ảnh copy trong Explorer.
/// </summary>
public static class ClipboardImageReader
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp"
    };

    /// <summary>
    /// Đọc ảnh từ Clipboard hiện tại của hệ thống (phải gọi trên luồng STA).
    /// </summary>
    public static BitmapSource? ReadFromClipboard()
    {
        try
        {
            return Read(Clipboard.GetDataObject());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Clipboard] Không đọc được Clipboard: {ex.Message}");
            return null;
        }
    }

    public static BitmapSource? Read(IDataObject? data)
    {
        if (data == null) return null;

        return TryReadPng(data) ?? TryReadBitmap(data) ?? TryReadFileDrop(data);
    }

    private static BitmapSource? TryReadPng(IDataObject data)
    {
        if (!data.GetDataPresent("PNG") || data.GetData("PNG") is not Stream stream) return null;

        try
        {
            stream.Position = 0;
            var frame = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            frame.Freeze();
            return frame;
        }
        catch
        {
            return null;
        }
    }

    private static BitmapSource? TryReadBitmap(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.Bitmap) || data.GetData(DataFormats.Bitmap) is not BitmapSource bitmap) return null;

        // Sao chép để không giữ tham chiếu tới dữ liệu Clipboard (có thể bị app khác thay đổi)
        var copy = new WriteableBitmap(bitmap);
        copy.Freeze();
        return copy;
    }

    private static BitmapSource? TryReadFileDrop(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop) || data.GetData(DataFormats.FileDrop) is not string[] files) return null;

        foreach (var path in files)
        {
            if (!ImageExtensions.Contains(Path.GetExtension(path)) || !File.Exists(path)) continue;

            try
            {
                using var stream = new MemoryStream(File.ReadAllBytes(path));
                var frame = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                frame.Freeze();
                return frame;
            }
            catch
            {
                // File hỏng hoặc định dạng không hỗ trợ -> thử file tiếp theo
            }
        }

        return null;
    }
}
