using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Đăng ký phím tắt toàn cục theo AppSettings, hỗ trợ đổi phím khi đang chạy (không cần khởi động lại).
/// </summary>
public class AppHotkeyManager : IAppHotkeyManager
{
    private readonly IHotkeyService _hotkeyService;
    private readonly Dictionary<HotkeyAction, Action> _handlers = new();
    private readonly Dictionary<HotkeyAction, (int Id, HotkeyGesture Gesture)> _active = new();
    private bool _suspended;

    public AppHotkeyManager(IHotkeyService hotkeyService)
    {
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
    }

    /// <summary>
    /// Phím tắt mặc định của từng hành động (dùng khi settings trống hoặc không hợp lệ).
    /// </summary>
    public static string DefaultGestureText(HotkeyAction action) => action switch
    {
        HotkeyAction.Capture => "Ctrl+Shift+A",
        HotkeyAction.CaptureAlternate => "PrintScreen",
        HotkeyAction.PinFromClipboard => "F3",
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };

    /// <summary>
    /// Đọc chuỗi phím tắt của một hành động trong settings.
    /// </summary>
    public static string? GestureText(AppSettings settings, HotkeyAction action) => action switch
    {
        HotkeyAction.Capture => settings.HotkeyCapture,
        HotkeyAction.CaptureAlternate => settings.HotkeyCaptureAlternate,
        HotkeyAction.PinFromClipboard => settings.HotkeyPinClipboard,
        _ => null
    };

    public void SetHandler(HotkeyAction action, Action handler)
    {
        _handlers[action] = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public IReadOnlyList<HotkeyAction> Apply(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Resume();

        var failed = new List<HotkeyAction>();
        foreach (var (action, handler) in _handlers)
        {
            if (!HotkeyGesture.TryParse(GestureText(settings, action), out var desired))
            {
                HotkeyGesture.TryParse(DefaultGestureText(action), out desired);
            }

            if (_active.TryGetValue(action, out var current) && current.Gesture == desired) continue;

            if (_active.Remove(action, out var previous))
            {
                _hotkeyService.UnregisterHotkey(previous.Id);
            }

            var id = _hotkeyService.RegisterHotkey(desired, handler);
            if (id > 0)
            {
                _active[action] = (id, desired);
                continue;
            }

            failed.Add(action);

            // Không chiếm được phím mới -> khôi phục phím cũ để người dùng vẫn chụp được
            if (previous.Id > 0)
            {
                var restoredId = _hotkeyService.RegisterHotkey(previous.Gesture, handler);
                if (restoredId > 0) _active[action] = (restoredId, previous.Gesture);
            }
        }

        return failed;
    }

    public HotkeyGesture? GetActiveGesture(HotkeyAction action) =>
        _active.TryGetValue(action, out var entry) ? entry.Gesture : null;

    public void Suspend()
    {
        if (_suspended) return;
        _suspended = true;

        foreach (var (id, _) in _active.Values)
        {
            _hotkeyService.UnregisterHotkey(id);
        }
    }

    public void Resume()
    {
        if (!_suspended) return;
        _suspended = false;

        foreach (var action in _active.Keys.ToList())
        {
            var gesture = _active[action].Gesture;
            var id = _hotkeyService.RegisterHotkey(gesture, _handlers[action]);
            if (id > 0) _active[action] = (id, gesture);
            else _active.Remove(action);
        }
    }
}
