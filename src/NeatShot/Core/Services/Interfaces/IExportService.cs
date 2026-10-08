using NeatShot.Core.Models;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Dịch vụ xuất ảnh chụp hoàn thiện: ghép ảnh crop với các nét vẽ vector,
/// lưu file PNG xuống ổ cứng và sao chép vào Clipboard.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Ghép ảnh nền crop với toàn bộ nét vẽ vector trong vùng chọn thành ảnh RenderTargetBitmap sắc nét.
    /// Hỗ trợ tùy chọn làm đẹp ảnh BeautifyOptions (khoảng đệm, bo góc, nền gradient).
    /// </summary>
    RenderTargetBitmap RenderFinalImage(
        BitmapSource background,
        CaptureRegion region,
        IEnumerable<DrawingElement> annotations,
        BeautifyOptions? beautifyOptions = null);

    /// <summary>
    /// Sao chép ảnh vào Windows Clipboard trên luồng STA Thread an toàn.
    /// </summary>
    Task CopyToClipboardAsync(BitmapSource image);

    /// <summary>
    /// Lưu ảnh dưới định dạng file PNG xuống đường dẫn chỉ định.
    /// </summary>
    Task SaveToFileAsync(BitmapSource image, string filePath);
}
