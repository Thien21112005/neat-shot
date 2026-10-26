using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Presentation.ViewModels;
using NeatShot.Tests.Fakes;
using System.IO;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.ViewModels;

public class SettingsViewModelTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), "NeatShot_SettingsVm_" + Guid.NewGuid().ToString("N") + ".json");
    private readonly FakeHotkeyService _hotkeys = new();
    private readonly FakeStartupService _startup = new();
    private readonly SettingsService _settings;
    private readonly AppHotkeyManager _manager;

    private static readonly HotkeyGesture CtrlShiftA = new(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
    private static readonly HotkeyGesture CtrlAltS = new(Key.S, ModifierKeys.Control | ModifierKeys.Alt);

    public SettingsViewModelTests()
    {
        _settings = new SettingsService(_settingsPath);
        _settings.LoadSettings();
        _manager = new AppHotkeyManager(_hotkeys);
        _manager.SetHandler(Core.Services.Interfaces.HotkeyAction.Capture, () => { });
        _manager.SetHandler(Core.Services.Interfaces.HotkeyAction.CaptureAlternate, () => { });
        _manager.Apply(_settings.CurrentSettings);
    }

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* bỏ qua */ }
    }

    private SettingsViewModel CreateViewModel() =>
        new(_settings, _startup, _manager, () => @"C:\Apps\NeatShot\NeatShot.exe");

    [Fact]
    public void LoadsCurrentValues_FromSettingsAndStartupRegistration()
    {
        _startup.Enable(@"C:\Apps\NeatShot\NeatShot.exe");

        var vm = CreateViewModel();

        Assert.Equal(CtrlShiftA, vm.CaptureHotkey);
        Assert.Equal(new HotkeyGesture(Key.PrintScreen, ModifierKeys.None), vm.CaptureAlternateHotkey);
        Assert.True(vm.AutoSaveToGallery);
        Assert.True(vm.IsPng);
        Assert.True(vm.LaunchAtStartup);
        Assert.False(vm.AutoCopyOnCapture);
    }

    [Fact]
    public void Save_PersistsEveryOption_AndAppliesTheNewHotkeyImmediately()
    {
        var vm = CreateViewModel();
        vm.CaptureHotkey = CtrlAltS;
        vm.SaveDirectory = @"D:\Shots";
        vm.AutoSaveToGallery = false;
        vm.IsJpg = true;
        vm.JpegQuality = 75;
        vm.LaunchAtStartup = true;
        vm.AutoCopyOnCapture = true;

        Assert.True(vm.Save());

        var saved = new SettingsService(_settingsPath).LoadSettings();
        Assert.Equal("Ctrl+Alt+S", saved.HotkeyCapture);
        Assert.Equal(@"D:\Shots", saved.DefaultSaveDirectory);
        Assert.False(saved.AutoSaveToGallery);
        Assert.Equal("jpg", saved.ImageFormat);
        Assert.Equal(75, saved.JpegQuality);
        Assert.True(saved.LaunchAtStartup);
        Assert.True(saved.AutoCopyOnCapture);
        Assert.Contains(_hotkeys.Registered.Values, r => r.Gesture == CtrlAltS);
        Assert.Equal(@"C:\Apps\NeatShot\NeatShot.exe", _startup.RegisteredPath);
        Assert.False(vm.HasError);
    }

    [Fact]
    public void Save_KeepsTheOldHotkey_AndWarns_WhenTheNewOneIsTakenByAnotherApp()
    {
        _hotkeys.TakenByOtherApps.Add(CtrlAltS);
        var vm = CreateViewModel();
        vm.CaptureHotkey = CtrlAltS;

        Assert.False(vm.Save());

        Assert.True(vm.HasError);
        Assert.Contains("Ctrl + Alt + S", vm.StatusMessage);
        Assert.Equal(CtrlShiftA, vm.CaptureHotkey);
        Assert.Equal("Ctrl+Shift+A", new SettingsService(_settingsPath).LoadSettings().HotkeyCapture);
    }

    [Fact]
    public void Save_RejectsUsingTheSameGestureForBothCaptureKeys()
    {
        var vm = CreateViewModel();
        vm.CaptureAlternateHotkey = CtrlShiftA;

        Assert.False(vm.Save());
        Assert.True(vm.HasError);
    }

    [Fact]
    public void TurningOffLaunchAtStartup_RemovesTheRegistration()
    {
        _startup.Enable(@"C:\Apps\NeatShot\NeatShot.exe");
        var vm = CreateViewModel();
        vm.LaunchAtStartup = false;

        vm.Save();

        Assert.Null(_startup.RegisteredPath);
        Assert.False(new SettingsService(_settingsPath).LoadSettings().LaunchAtStartup);
    }

    [Fact]
    public void ResetHotkeys_RestoresTheDefaults()
    {
        var vm = CreateViewModel();
        vm.CaptureHotkey = CtrlAltS;

        vm.ResetHotkeysCommand.Execute(null);

        Assert.Equal(CtrlShiftA, vm.CaptureHotkey);
        Assert.Equal(new HotkeyGesture(Key.PrintScreen, ModifierKeys.None), vm.CaptureAlternateHotkey);
    }

    [Fact]
    public void PinFromClipboardHotkey_DefaultsToF3_AndCanBeChanged()
    {
        _manager.SetHandler(Core.Services.Interfaces.HotkeyAction.PinFromClipboard, () => { });
        _manager.Apply(_settings.CurrentSettings);
        var vm = CreateViewModel();
        Assert.Equal(new HotkeyGesture(Key.F3, ModifierKeys.None), vm.PinClipboardHotkey);

        vm.PinClipboardHotkey = new HotkeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Alt);

        Assert.True(vm.Save());
        Assert.Equal("Ctrl+Alt+V", new SettingsService(_settingsPath).LoadSettings().HotkeyPinClipboard);
        Assert.Contains(_hotkeys.Registered.Values, r => r.Gesture == new HotkeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Alt));
    }

    [Fact]
    public void Save_RejectsAnyTwoActionsSharingAGesture()
    {
        var vm = CreateViewModel();
        vm.PinClipboardHotkey = CtrlShiftA;

        Assert.False(vm.Save());
        Assert.True(vm.HasError);
    }
}
