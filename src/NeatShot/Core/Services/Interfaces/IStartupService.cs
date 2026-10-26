namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Quản lý việc tự khởi động NeatShot cùng Windows (chạy nền ở khay hệ thống sau khi đăng nhập).
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// NeatShot hiện có được đăng ký khởi động cùng Windows hay không.
    /// </summary>
    bool IsEnabled();

    /// <summary>
    /// Đăng ký (hoặc cập nhật đường dẫn) file exe để chạy khi người dùng đăng nhập Windows.
    /// </summary>
    void Enable(string exePath);

    /// <summary>
    /// Gỡ đăng ký khởi động cùng Windows. Gọi nhiều lần vẫn an toàn.
    /// </summary>
    void Disable();
}
