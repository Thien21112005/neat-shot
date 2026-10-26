using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.Windows.Input;

namespace NeatShot.Tests.Fakes;

/// <summary>
/// Hotkey service giả: ghi nhận phím đã đăng ký và mô phỏng phím bị ứng dụng khác chiếm,
/// không gọi RegisterHotKey thật của Windows.
/// </summary>
public sealed class FakeHotkeyService : IHotkeyService
{
    private int _nextId = 1;

    public Dictionary<int, (HotkeyGesture Gesture, Action Callback)> Registered { get; } = new();
    public HashSet<HotkeyGesture> TakenByOtherApps { get; } = new();

    public event EventHandler? HotkeyPressed { add { } remove { } }
    public bool Register(Key key, ModifierKeys modifiers) => true;
    public void Unregister() => Registered.Clear();
    public bool IsRegistered => Registered.Count > 0;
    public Key RegisteredKey => Key.None;
    public ModifierKeys RegisteredModifiers => ModifierKeys.None;

    public int RegisterHotkey(HotkeyGesture gesture, Action callback)
    {
        if (!gesture.IsValid || TakenByOtherApps.Contains(gesture) || Registered.Values.Any(r => r.Gesture == gesture)) return 0;
        var id = _nextId++;
        Registered[id] = (gesture, callback);
        return id;
    }

    public void UnregisterHotkey(int id) => Registered.Remove(id);

    public void Press(HotkeyGesture gesture) => Registered.Values.Single(r => r.Gesture == gesture).Callback();

    public void Dispose()
    {
    }
}
