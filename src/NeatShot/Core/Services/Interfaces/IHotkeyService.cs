using System.Windows.Input;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Quản lý việc đăng ký và lắng nghe phím tắt toàn cục (Global Hotkey) trên Windows.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Kích hoạt khi người dùng nhấn phím tắt toàn cục đã đăng ký.
    /// </summary>
    event EventHandler HotkeyPressed;

    /// <summary>
    /// Đăng ký một phím tắt toàn cục mới.
    /// </summary>
    bool Register(Key key, ModifierKeys modifiers);

    /// <summary>
    /// Hủy đăng ký phím tắt hiện tại.
    /// </summary>
    void Unregister();

    /// <summary>
    /// Cho biết phím tắt hiện tại đã đăng ký thành công hay chưa.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Phím chính đã đăng ký.
    /// </summary>
    Key RegisteredKey { get; }

    /// <summary>
    /// Các phím bổ trợ (Ctrl, Shift, Alt, Win) đã đăng ký.
    /// </summary>
    ModifierKeys RegisteredModifiers { get; }
}
