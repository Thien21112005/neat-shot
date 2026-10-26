using NeatShot.Core.Interop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace NeatShot.Presentation.Tray;

/// <summary>
/// Mở menu chuột phải của icon khay sao cho nó tự đóng khi người dùng click ra ngoài.
/// </summary>
/// <remarks>
/// NeatShot chạy nền nên khi menu mở, app không phải cửa sổ đang được kích hoạt: Windows không báo cho popup
/// biết người dùng đã click nơi khác, và menu bị "dính" trên màn hình. Cách chuẩn (giống TrackPopupMenu):
/// đưa cửa sổ của menu lên foreground khi mở, rồi đóng menu khi cửa sổ đó mất kích hoạt (WM_ACTIVATE = WA_INACTIVE).
/// </remarks>
public sealed class TrayMenuController
{
    private const int WM_ACTIVATE = 0x0006;
    private const int WA_INACTIVE = 0;

    private readonly ContextMenu _menu;
    private HwndSource? _popupSource;

    public TrayMenuController(ContextMenu menu)
    {
        _menu = menu ?? throw new ArgumentNullException(nameof(menu));
        _menu.Opened += OnOpened;
        _menu.Closed += OnClosed;
    }

    /// <summary>
    /// Handle cửa sổ popup của menu khi đang mở (dùng cho test).
    /// </summary>
    internal IntPtr PopupHandle => _popupSource?.Handle ?? IntPtr.Zero;

    /// <summary>
    /// Mở menu tại vị trí con trỏ chuột.
    /// </summary>
    public void Open()
    {
        _menu.Placement = PlacementMode.MousePoint;
        _menu.IsOpen = true;
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
        if (PresentationSource.FromVisual(_menu) is not HwndSource source) return;

        _popupSource = source;
        _popupSource.AddHook(PopupHook);
        NativeMethods.SetForegroundWindow(source.Handle);
    }

    private void OnClosed(object sender, RoutedEventArgs e)
    {
        _popupSource?.RemoveHook(PopupHook);
        _popupSource = null;
    }

    private IntPtr PopupHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_ACTIVATE && (wParam.ToInt64() & 0xFFFF) == WA_INACTIVE && _menu.IsOpen)
        {
            _menu.IsOpen = false;
        }

        return IntPtr.Zero;
    }
}
