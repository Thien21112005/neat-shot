using NeatShot.Core.Models;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Dịch vụ quản lý kho lưu trữ và thư viện ảnh chụp màn hình NeatShot.
/// </summary>
public interface IScreenshotGalleryService
{
    /// <summary>
    /// Lấy đường dẫn thư mục lưu ảnh chụp màn hình (mặc định: C:\Users\<Username>\Pictures\NeatShot).
    /// Tự động tạo thư mục nếu chưa tồn tại.
    /// </summary>
    string GetScreenshotsDirectory();

    /// <summary>
    /// Nạp danh sách các ảnh chụp có trong thư mục, sắp xếp từ mới nhất đến cũ nhất.
    /// </summary>
    Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync();

    /// <summary>
    /// Lưu ảnh chụp màn hình vào thư mục dưới định dạng PNG.
    /// </summary>
    Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null);

    /// <summary>
    /// Xóa một ảnh khỏi thư mục lưu trữ.
    /// </summary>
    bool DeleteScreenshot(string filePath);

    /// <summary>
    /// Nạp đối tượng BitmapSource từ file mà không khóa file trên ổ đĩa.
    /// </summary>
    BitmapSource? LoadImage(string filePath);
}
