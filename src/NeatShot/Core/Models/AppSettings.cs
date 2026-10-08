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
    /// Thư mục lưu ảnh mặc định khi người dùng chọn lưu file.
    /// </summary>
    public string DefaultSaveDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Phím tắt kích hoạt chụp ảnh màn hình (mặc định: Ctrl+Shift+A).
    /// </summary>
    public string HotkeyCapture { get; set; } = "Ctrl+Shift+A";

    /// <summary>
    /// Tự động copy ảnh vào Clipboard sau khi hoàn tất khoanh vùng.
    /// </summary>
    public bool AutoCopyOnCapture { get; set; } = false;
}
