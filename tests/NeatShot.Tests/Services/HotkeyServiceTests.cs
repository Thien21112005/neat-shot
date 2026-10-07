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
}
