using NeatShot.Core.Models;
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
    /// Đăng ký một phím tắt toàn cục gắn với hành động riêng.
    /// </summary>
    /// <returns>Id của phím tắt (&gt; 0), hoặc 0 nếu tổ hợp không hợp lệ hoặc đã bị ứng dụng khác chiếm.</returns>
    int RegisterHotkey(HotkeyGesture gesture, Action callback);

    /// <summary>
    /// Hủy một phím tắt đã đăng ký bằng <see cref="RegisterHotkey"/>.
    /// </summary>
    void UnregisterHotkey(int id);

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
