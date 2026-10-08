using NeatShot.Core.Models;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Dịch vụ quản lý nạp và lưu trữ cấu hình người dùng NeatShot vào file JSON.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Đối tượng cấu hình hiện tại đang được sử dụng trong bộ nhớ.
    /// </summary>
    AppSettings CurrentSettings { get; }

    /// <summary>
    /// Nạp cấu hình từ file JSON (nếu không tồn tại hoặc lỗi, trả về cấu hình mặc định).
    /// </summary>
    AppSettings LoadSettings();

    /// <summary>
    /// Lưu cấu hình hiện tại xuống file JSON đồng bộ.
    /// </summary>
    void SaveSettings(AppSettings settings);

    /// <summary>
    /// Lưu cấu hình hiện tại xuống file JSON bất đồng bộ.
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);
}
