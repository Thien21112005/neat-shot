using System.Windows;
using System.Windows.Media.Imaging;
using NeatShot.Core.Models;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Giao diện dịch vụ chụp ảnh màn hình, xử lý chụp sạch và crop ảnh.
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>
    /// Lấy toạ độ và kích thước bao phủ toàn bộ các màn hình ảo (Virtual Screen).
    /// </summary>
    Rect GetVirtualScreenBounds();

    /// <summary>
    /// Chụp ảnh sạch toàn màn hình (gửi tín hiệu ẩn popup, chờ 120ms và chụp GDI).
    /// </summary>
    Task<BitmapSource> CaptureCleanScreenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Chụp toàn bộ màn hình ảo. <paramref name="dismissPopups"/> = true sẽ đóng flyout/popup trước khi chụp
    /// (Clean Shot); false giữ nguyên màn hình — dùng cho chụp hẹn giờ để chụp được menu đang mở.
    /// </summary>
    Task<BitmapSource> CaptureScreenAsync(bool dismissPopups, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cắt vùng ảnh từ bitmap gốc theo toạ độ CaptureRegion.
    /// </summary>
    BitmapSource Crop(BitmapSource source, CaptureRegion region);
}
