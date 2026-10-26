using System.Windows.Input;

namespace NeatShot.Core.Models;

/// <summary>
/// Một tổ hợp phím tắt toàn cục (ví dụ Ctrl + Shift + A), lưu trong settings dưới dạng chuỗi "Ctrl+Shift+A".
/// </summary>
public readonly record struct HotkeyGesture(Key Key, ModifierKeys Modifiers)
{
    private static readonly (ModifierKeys Flag, string Name)[] ModifierOrder =
    {
        (ModifierKeys.Control, "Ctrl"),
        (ModifierKeys.Shift, "Shift"),
        (ModifierKeys.Alt, "Alt"),
        (ModifierKeys.Windows, "Win")
    };

    /// <summary>
    /// Hợp lệ khi có phím chính (không phải phím bổ trợ) và không chặn việc gõ chữ thông thường:
    /// phím chữ/số/ký tự phải đi kèm Ctrl, Alt hoặc Win; phím chức năng (F1–F24, PrtSc…) dùng một mình được.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (Key == Key.None || IsModifierKey(Key)) return false;
            if (IsStandaloneKey(Key)) return true;
            return (Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0;
        }
    }

    /// <summary>
    /// Dạng hiển thị cho người dùng, ví dụ "Ctrl + Shift + A".
    /// </summary>
    public string DisplayText => string.Join(" + ", Parts(friendly: true));

    /// <summary>
    /// Dạng chuẩn để lưu vào settings, ví dụ "Ctrl+Shift+A".
    /// </summary>
    public override string ToString() => string.Join("+", Parts(friendly: false));

    public static bool TryParse(string? text, out HotkeyGesture gesture)
    {
        gesture = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var modifiers = ModifierKeys.None;
        Key? key = null;

        foreach (var raw in text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (raw.ToLowerInvariant())
            {
                case "ctrl" or "control":
                    modifiers |= ModifierKeys.Control;
                    continue;
                case "shift":
                    modifiers |= ModifierKeys.Shift;
                    continue;
                case "alt":
                    modifiers |= ModifierKeys.Alt;
                    continue;
                case "win" or "windows":
                    modifiers |= ModifierKeys.Windows;
                    continue;
            }

            if (key != null || !TryParseKey(raw, out var parsed)) return false;
            key = parsed;
        }

        if (key == null) return false;

        gesture = new HotkeyGesture(key.Value, modifiers);
        return gesture.IsValid;
    }

    private IEnumerable<string> Parts(bool friendly)
    {
        foreach (var (flag, name) in ModifierOrder)
        {
            if (Modifiers.HasFlag(flag)) yield return name;
        }

        yield return KeyName(Key, friendly);
    }

    private static string KeyName(Key key, bool friendly) => key switch
    {
        >= Key.D0 and <= Key.D9 => ((int)(key - Key.D0)).ToString(),
        Key.PrintScreen => friendly ? "PrtSc" : "PrintScreen",
        Key.OemPlus => "Plus",
        Key.OemMinus => "Minus",
        _ => key.ToString()
    };

    private static bool TryParseKey(string text, out Key key)
    {
        key = Key.None;
        if (text.Length == 1 && char.IsDigit(text[0]))
        {
            key = Key.D0 + (text[0] - '0');
            return true;
        }

        switch (text.ToLowerInvariant())
        {
            case "prtsc" or "printscreen" or "prtscn":
                key = Key.PrintScreen;
                return true;
            case "plus":
                key = Key.OemPlus;
                return true;
            case "minus":
                key = Key.OemMinus;
                return true;
        }

        return Enum.TryParse(text, ignoreCase: true, out key) && Enum.IsDefined(key) && key != Key.None;
    }

    private static bool IsModifierKey(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
        Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.System;

    private static bool IsStandaloneKey(Key key) =>
        key is >= Key.F1 and <= Key.F24 || key is Key.PrintScreen or Key.Pause or Key.Scroll;
}
