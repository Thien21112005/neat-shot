using NeatShot.Core.Models;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Các hành động có thể gán phím tắt toàn cục.
/// </summary>
public enum HotkeyAction
{
    /// <summary>Chụp vùng chọn.</summary>
    Capture,

    /// <summary>Phím chụp phụ (mặc định PrtSc).</summary>
    CaptureAlternate,

    /// <summary>Ghim ảnh đang có trong Clipboard lên màn hình (mặc định F3).</summary>
    PinFromClipboard
}

/// <summary>
/// Đăng ký các phím tắt toàn cục của NeatShot theo cấu hình người dùng (AppSettings).
/// </summary>
public interface IAppHotkeyManager
{
    /// <summary>
    /// Gán hành động sẽ chạy khi phím tắt của <paramref name="action"/> được nhấn.
    /// </summary>
    void SetHandler(HotkeyAction action, Action handler);

    /// <summary>
    /// Đăng ký (hoặc đăng ký lại) phím tắt theo settings. Phím mới bị ứng dụng khác chiếm thì giữ nguyên phím cũ.
    /// </summary>
    /// <returns>Danh sách hành động không đăng ký được phím mới.</returns>
    IReadOnlyList<HotkeyAction> Apply(AppSettings settings);

    /// <summary>
    /// Phím tắt đang có hiệu lực của một hành động (null nếu chưa đăng ký được).
    /// </summary>
    HotkeyGesture? GetActiveGesture(HotkeyAction action);

    /// <summary>
    /// Tạm nhả toàn bộ phím tắt (ví dụ khi người dùng đang bấm tổ hợp mới trong cửa sổ Cài đặt).
    /// </summary>
    void Suspend();

    /// <summary>
    /// Đăng ký lại các phím tắt đã nhả bởi <see cref="Suspend"/>.
    /// </summary>
    void Resume();
}
