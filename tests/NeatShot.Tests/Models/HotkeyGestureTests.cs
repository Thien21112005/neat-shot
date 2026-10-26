using NeatShot.Core.Models;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Models;

public class HotkeyGestureTests
{
    [Theory]
    [InlineData("Ctrl+Shift+A", Key.A, ModifierKeys.Control | ModifierKeys.Shift)]
    [InlineData("ctrl + shift + a", Key.A, ModifierKeys.Control | ModifierKeys.Shift)]
    [InlineData("Control+Alt+S", Key.S, ModifierKeys.Control | ModifierKeys.Alt)]
    [InlineData("Win+Shift+3", Key.D3, ModifierKeys.Windows | ModifierKeys.Shift)]
    [InlineData("F3", Key.F3, ModifierKeys.None)]
    [InlineData("PrintScreen", Key.PrintScreen, ModifierKeys.None)]
    [InlineData("PrtSc", Key.PrintScreen, ModifierKeys.None)]
    public void TryParse_ReadsModifiersAndKey(string text, Key key, ModifierKeys modifiers)
    {
        Assert.True(HotkeyGesture.TryParse(text, out var gesture));
        Assert.Equal(key, gesture.Key);
        Assert.Equal(modifiers, gesture.Modifiers);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ctrl+Shift")]
    [InlineData("Ctrl+NotAKey")]
    [InlineData("A")]          // Phím chữ không kèm phím bổ trợ sẽ chặn việc gõ chữ
    [InlineData("Shift+A")]    // Shift + chữ vẫn là gõ chữ hoa
    public void TryParse_RejectsInvalidOrUnsafeGestures(string? text)
    {
        Assert.False(HotkeyGesture.TryParse(text, out _));
    }

    [Theory]
    [InlineData(Key.A, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+A", "Ctrl + Shift + A")]
    [InlineData(Key.D3, ModifierKeys.Windows | ModifierKeys.Alt, "Alt+Win+3", "Alt + Win + 3")]
    [InlineData(Key.PrintScreen, ModifierKeys.None, "PrintScreen", "PrtSc")]
    [InlineData(Key.F3, ModifierKeys.None, "F3", "F3")]
    public void ToString_IsCanonical_AndDisplayTextIsFriendly(Key key, ModifierKeys modifiers, string canonical, string display)
    {
        var gesture = new HotkeyGesture(key, modifiers);

        Assert.Equal(canonical, gesture.ToString());
        Assert.Equal(display, gesture.DisplayText);
        Assert.True(HotkeyGesture.TryParse(gesture.ToString(), out var roundTrip));
        Assert.Equal(gesture, roundTrip);
    }

    [Theory]
    [InlineData(Key.LeftCtrl, ModifierKeys.Control, false)]
    [InlineData(Key.A, ModifierKeys.None, false)]
    [InlineData(Key.A, ModifierKeys.Alt, true)]
    [InlineData(Key.F10, ModifierKeys.None, true)]
    [InlineData(Key.None, ModifierKeys.Control, false)]
    public void IsValid_RequiresARealKeyAndSafeModifiers(Key key, ModifierKeys modifiers, bool expected)
    {
        Assert.Equal(expected, new HotkeyGesture(key, modifiers).IsValid);
    }
}
