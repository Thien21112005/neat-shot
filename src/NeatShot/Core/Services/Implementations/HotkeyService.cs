using NeatShot.Core.Interop;
using NeatShot.Core.Services.Interfaces;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Quản lý phím tắt toàn cục bằng Win32 RegisterHotKey và tạo cửa sổ ngầm HwndSource để bắt WM_HOTKEY.
/// </summary>
public class HotkeyService : IHotkeyService
{
    private const int HotkeyId = 9001;
    private HwndSource? _hwndSource;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;
    public bool IsRegistered { get; private set; }
    public Key RegisteredKey { get; private set; } = Key.None;
    public ModifierKeys RegisteredModifiers { get; private set; } = ModifierKeys.None;

    public bool Register(Key key, ModifierKeys modifiers)
    {
        Unregister();

        if (key == Key.None) return false;

        EnsureHwndSource();
        if (_hwndSource == null || _hwndSource.Handle == IntPtr.Zero) return false;

        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        uint fsModifiers = NativeConstants.MOD_NOREPEAT;

        if (modifiers.HasFlag(ModifierKeys.Alt)) fsModifiers |= NativeConstants.MOD_ALT;
        if (modifiers.HasFlag(ModifierKeys.Control)) fsModifiers |= NativeConstants.MOD_CONTROL;
        if (modifiers.HasFlag(ModifierKeys.Shift)) fsModifiers |= NativeConstants.MOD_SHIFT;
        if (modifiers.HasFlag(ModifierKeys.Windows)) fsModifiers |= NativeConstants.MOD_WIN;

        var success = NativeMethods.RegisterHotKey(_hwndSource.Handle, HotkeyId, fsModifiers, vk);
        if (success)
        {
            IsRegistered = true;
            RegisteredKey = key;
            RegisteredModifiers = modifiers;
        }

        return success;
    }

    public void Unregister()
    {
        if (IsRegistered && _hwndSource != null && _hwndSource.Handle != IntPtr.Zero)
        {
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, HotkeyId);
        }

        IsRegistered = false;
        RegisteredKey = Key.None;
        RegisteredModifiers = ModifierKeys.None;
    }

    private void EnsureHwndSource()
    {
        if (_hwndSource != null) return;

        try
        {
            var parameters = new HwndSourceParameters("NeatShotHotkeySink")
            {
                WindowStyle = 0,
                PositionX = 0,
                PositionY = 0,
                Width = 0,
                Height = 0
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(HwndHook);
        }
        catch
        {
            // Bỏ qua nếu chạy trong môi trường unit test không có UI message loop
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeConstants.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Unregister();

        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(HwndHook);
            _hwndSource.Dispose();
            _hwndSource = null;
        }

        GC.SuppressFinalize(this);
    }
}
