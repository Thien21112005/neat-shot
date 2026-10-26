using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Tests.Fakes;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Services;

public class AppHotkeyManagerTests
{
    private static readonly HotkeyGesture CtrlShiftA = new(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
    private static readonly HotkeyGesture CtrlAltS = new(Key.S, ModifierKeys.Control | ModifierKeys.Alt);

    [Fact]
    public void Apply_RegistersTheCaptureHotkeyFromSettings_AndRunsItsHandler()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        var captured = 0;
        manager.SetHandler(HotkeyAction.Capture, () => captured++);

        var failed = manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Shift+A" });

        Assert.Empty(failed);
        Assert.Equal(CtrlShiftA, manager.GetActiveGesture(HotkeyAction.Capture));
        hotkeys.Press(CtrlShiftA);
        Assert.Equal(1, captured);
    }

    [Fact]
    public void Apply_ChangingTheHotkey_ReplacesTheOldRegistration()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });
        manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Shift+A" });

        var failed = manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Alt+S" });

        Assert.Empty(failed);
        Assert.Single(hotkeys.Registered);
        Assert.Equal(CtrlAltS, hotkeys.Registered.Values.Single().Gesture);
    }

    [Fact]
    public void Apply_KeepsThePreviousHotkey_WhenTheNewOneIsTakenByAnotherApp()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });
        manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Shift+A" });
        hotkeys.TakenByOtherApps.Add(CtrlAltS);

        var failed = manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Alt+S" });

        Assert.Equal(new[] { HotkeyAction.Capture }, failed);
        Assert.Equal(CtrlShiftA, manager.GetActiveGesture(HotkeyAction.Capture));
        Assert.Equal(CtrlShiftA, hotkeys.Registered.Values.Single().Gesture);
    }

    [Fact]
    public void Apply_FallsBackToTheDefault_WhenTheSavedTextIsInvalid()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });

        manager.Apply(new AppSettings { HotkeyCapture = "garbage" });

        Assert.Equal(CtrlShiftA, manager.GetActiveGesture(HotkeyAction.Capture));
    }

    [Fact]
    public void Apply_RegistersTheAlternateCaptureKey_PrintScreenByDefault()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });
        manager.SetHandler(HotkeyAction.CaptureAlternate, () => { });

        manager.Apply(new AppSettings());

        Assert.Equal(new HotkeyGesture(Key.PrintScreen, ModifierKeys.None), manager.GetActiveGesture(HotkeyAction.CaptureAlternate));
        Assert.Equal(2, hotkeys.Registered.Count);
    }

    [Fact]
    public void SuspendAndResume_ReleaseAndRestoreAllHotkeys_SoTheyCanBeRecorded()
    {
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });
        manager.Apply(new AppSettings { HotkeyCapture = "Ctrl+Shift+A" });

        manager.Suspend();
        Assert.Empty(hotkeys.Registered);

        manager.Resume();
        Assert.Equal(CtrlShiftA, hotkeys.Registered.Values.Single().Gesture);
        Assert.Equal(CtrlShiftA, manager.GetActiveGesture(HotkeyAction.Capture));
    }
}
