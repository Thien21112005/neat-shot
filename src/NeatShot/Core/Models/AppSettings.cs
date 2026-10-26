using System.IO;

namespace NeatShot.Core.Models;

/// <summary>
/// Đại diện cho cấu hình người dùng của ứng dụng NeatShot.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Danh sách mã màu Hex của các màu người dùng đã sử dụng gần nhất (Color History).
    /// </summary>
    public List<string> RecentColorsHex { get; set; } = new();

    /// <summary>
    /// Thư mục lưu ảnh mặc định khi người dùng chọn lưu file (mặc định: Pictures\NeatShot).
    /// </summary>
    public string DefaultSaveDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "NeatShot");

    /// <summary>
    /// Phím tắt kích hoạt chụp ảnh màn hình (mặc định: Ctrl+Shift+A).
    /// </summary>
    public string HotkeyCapture { get; set; } = "Ctrl+Shift+A";

    /// <summary>
    /// Phím chụp phụ (mặc định: PrintScreen).
    /// </summary>
    public string HotkeyCaptureAlternate { get; set; } = "PrintScreen";

    /// <summary>
    /// Phím tắt ghim ảnh từ Clipboard lên màn hình (mặc định: F3).
    /// </summary>
    public string HotkeyPinClipboard { get; set; } = "F3";

    /// <summary>
    /// Tự động copy ảnh vào Clipboard sau khi hoàn tất khoanh vùng.
    /// </summary>
    public bool AutoCopyOnCapture { get; set; } = false;

    /// <summary>
    /// Tự lưu ảnh vào thư viện (thư mục lưu ảnh) mỗi khi Sao chép hoặc Ghim.
    /// </summary>
    public bool AutoSaveToGallery { get; set; } = true;

    /// <summary>
    /// Định dạng ảnh khi lưu: "png" (mặc định, không mất chất lượng) hoặc "jpg" (dung lượng nhỏ).
    /// </summary>
    public string ImageFormat { get; set; } = "png";

    /// <summary>
    /// Chất lượng nén JPG (1–100), chỉ dùng khi ImageFormat = "jpg".
    /// </summary>
    public int JpegQuality { get; set; } = 90;

    /// <summary>
    /// Tự khởi động cùng Windows và chạy nền ở khay hệ thống.
    /// null = người dùng chưa chọn (bản phát hành mặc định bật, giống Lightshot).
    /// </summary>
    public bool? LaunchAtStartup { get; set; }
}
