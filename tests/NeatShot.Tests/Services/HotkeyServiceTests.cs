using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using System;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Services;

public class HotkeyServiceTests
{
    [Fact]
    public void InitialState_ShouldNotBeRegistered()
    {
        using var service = new HotkeyService();

        Assert.False(service.IsRegistered);
        Assert.Equal(Key.None, service.RegisteredKey);
        Assert.Equal(ModifierKeys.None, service.RegisteredModifiers);
    }

    [Fact]
    public void Unregister_WhenNotRegistered_ShouldNotThrow()
    {
        using var service = new HotkeyService();

        var exception = Record.Exception(new Action(() => service.Unregister()));
        Assert.Null(exception);
    }

    // Tổ hợp hiếm gặp để không xung đột với phím tắt của NeatShot đang chạy trên máy
    private static readonly HotkeyGesture TestGesture = new(Key.F11, ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift);

    private static void RunOnSta(Action action)
    {
        Exception? error = null;
        var thread = new System.Threading.Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(error);
    }

    [Fact]
    public void RegisterHotkey_ReturnsId_AndRejectsTheSameGestureTwice()
    {
        RunOnSta(() =>
        {
            using var service = new HotkeyService();

            var first = service.RegisterHotkey(TestGesture, () => { });
            var duplicate = service.RegisterHotkey(TestGesture, () => { });

            Assert.True(first > 0);
            Assert.Equal(0, duplicate);
        });
    }

    [Fact]
    public void UnregisterHotkey_FreesTheGesture_ForANewRegistration()
    {
        RunOnSta(() =>
        {
            using var service = new HotkeyService();
            var id = service.RegisterHotkey(TestGesture, () => { });

            service.UnregisterHotkey(id);

            Assert.True(service.RegisterHotkey(TestGesture, () => { }) > 0);
        });
    }

    [Fact]
    public void RegisterHotkey_RejectsInvalidGestures()
    {
        RunOnSta(() =>
        {
            using var service = new HotkeyService();
            Assert.Equal(0, service.RegisterHotkey(new HotkeyGesture(Key.A, ModifierKeys.None), () => { }));
        });
    }
}
