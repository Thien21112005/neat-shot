using NeatShot.Core.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace NeatShot.Presentation.Tray;

/// <summary>
/// Quản lý biểu tượng khay hệ thống (System Tray) trực tiếp qua Win32 Shell_NotifyIcon và HwndSource.
/// Hoạt động 100% độc lập, không phụ thuộc thư viện bên ngoài hay Visual Tree của WPF.
/// </summary>
public class NativeTrayIcon : IDisposable
{
    private const int TrayIconId = 1001;

    // Explorer phát thông điệp này mỗi khi taskbar được tạo lại (ví dụ sau khi explorer.exe khởi động lại)
    private static readonly int TaskbarCreatedMessage = NativeMethods.RegisterWindowMessage("TaskbarCreated");

    private HwndSource? _hwndSource;
    private NOTIFYICONDATA _notifyData;
    private bool _isCreated;
    private bool _disposed;

    /// <summary>
    /// Handle cửa sổ ngầm nhận thông điệp từ khay hệ thống.
    /// </summary>
    internal IntPtr Handle => _hwndSource?.Handle ?? IntPtr.Zero;

    public event EventHandler? Click;
    public event EventHandler? ContextMenuRequested;

    public NativeTrayIcon(IntPtr hIcon, string tooltip)
    {
        var parameters = new HwndSourceParameters("NeatShotTraySink")
        {
            WindowStyle = 0,
            PositionX = 0,
            PositionY = 0,
            Width = 0,
            Height = 0
        };

        _hwndSource = new HwndSource(parameters);
        _hwndSource.AddHook(HwndHook);

        _notifyData = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwndSource.Handle,
            uID = TrayIconId,
            uFlags = NativeConstants.NIF_MESSAGE | NativeConstants.NIF_ICON | NativeConstants.NIF_TIP,
            uCallbackMessage = NativeConstants.WM_TRAYICON,
            hIcon = hIcon,
            szTip = tooltip
        };

        _isCreated = NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_ADD, ref _notifyData);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (TaskbarCreatedMessage != 0 && msg == TaskbarCreatedMessage && !_disposed)
        {
            // Icon cũ đã mất cùng taskbar cũ -> đăng ký lại để icon không biến mất
            _isCreated = NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_ADD, ref _notifyData);
            return IntPtr.Zero;
        }

        if (msg == NativeConstants.WM_TRAYICON && wParam.ToInt32() == TrayIconId)
        {
            var mouseMsg = lParam.ToInt32();
            if (mouseMsg == NativeConstants.WM_LBUTTONUP)
            {
                Click?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            else if (mouseMsg == NativeConstants.WM_RBUTTONUP)
            {
                ContextMenuRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Đổi chữ hiện khi rê chuột lên icon (ví dụ sau khi đổi ngôn ngữ).
    /// </summary>
    public void SetTooltip(string tooltip)
    {
        _notifyData.szTip = tooltip;
        if (_isCreated && !_disposed)
        {
            _notifyData.uFlags = NativeConstants.NIF_TIP;
            NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_MODIFY, ref _notifyData);
            _notifyData.uFlags = NativeConstants.NIF_MESSAGE | NativeConstants.NIF_ICON | NativeConstants.NIF_TIP;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_isCreated)
        {
            NativeMethods.Shell_NotifyIcon(NativeConstants.NIM_DELETE, ref _notifyData);
            _isCreated = false;
        }

        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(HwndHook);
            _hwndSource.Dispose();
            _hwndSource = null;
        }

        GC.SuppressFinalize(this);
    }
}
