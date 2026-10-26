using NeatShot.Common.Helpers;
using NeatShot.Core.Services.Implementations;
using Windows.ApplicationModel;
using Xunit;

namespace NeatShot.Tests.Services;

public class PackagedStartupServiceTests
{
    private sealed class FakeStartupTask : IStartupTaskHandle
    {
        public StartupTaskState State { get; set; } = StartupTaskState.Disabled;
        public int EnableRequests { get; private set; }

        /// <summary>Kết quả Windows trả về khi xin bật (mặc định: bật thành công).</summary>
        public StartupTaskState? EnableResult { get; set; }

        public StartupTaskState RequestEnable()
        {
            EnableRequests++;
            State = EnableResult ?? StartupTaskState.Enabled;
            return State;
        }

        public void Disable() => State = StartupTaskState.Disabled;
    }

    [Theory]
    [InlineData(StartupTaskState.Enabled, true)]
    [InlineData(StartupTaskState.EnabledByPolicy, true)]
    [InlineData(StartupTaskState.Disabled, false)]
    [InlineData(StartupTaskState.DisabledByUser, false)]
    [InlineData(StartupTaskState.DisabledByPolicy, false)]
    public void IsEnabled_FollowsTheStartupTaskState(StartupTaskState state, bool expected)
    {
        var service = new PackagedStartupService(new FakeStartupTask { State = state });
        Assert.Equal(expected, service.IsEnabled());
    }

    [Fact]
    public void Enable_RequestsTheStartupTask_AndIgnoresTheExePath()
    {
        var task = new FakeStartupTask();
        var service = new PackagedStartupService(task);

        service.Enable(@"C:\Program Files\WindowsApps\NeatShot\NeatShot.exe");

        Assert.Equal(1, task.EnableRequests);
        Assert.True(service.IsEnabled());
    }

    [Fact]
    public void Enable_DoesNothing_WhenAlreadyEnabled()
    {
        var task = new FakeStartupTask { State = StartupTaskState.Enabled };

        new PackagedStartupService(task).Enable("NeatShot.exe");

        Assert.Equal(0, task.EnableRequests);
    }

    [Fact]
    public void Enable_Throws_WhenTheUserTurnedItOffInWindowsSettings()
    {
        // Windows không cho app tự bật lại khi người dùng đã tắt trong Settings → Apps → Startup
        var task = new FakeStartupTask { State = StartupTaskState.DisabledByUser };

        var ex = Assert.Throws<InvalidOperationException>(() => new PackagedStartupService(task).Enable("NeatShot.exe"));

        Assert.Equal(0, task.EnableRequests);
        Assert.Contains("Startup", ex.Message);
    }

    [Fact]
    public void Enable_Throws_WhenWindowsRejectsTheRequest()
    {
        var task = new FakeStartupTask { EnableResult = StartupTaskState.DisabledByPolicy };

        Assert.Throws<InvalidOperationException>(() => new PackagedStartupService(task).Enable("NeatShot.exe"));
    }

    [Fact]
    public void Disable_TurnsTheStartupTaskOff()
    {
        var task = new FakeStartupTask { State = StartupTaskState.Enabled };

        new PackagedStartupService(task).Disable();

        Assert.Equal(StartupTaskState.Disabled, task.State);
    }

    [Fact]
    public void Create_UsesTheRunKey_WhenNotRunningFromAnMsixPackage()
    {
        // Tiến trình test không chạy trong gói MSIX
        Assert.False(AppPackage.IsPackaged);
        Assert.IsType<WindowsStartupService>(StartupServiceFactory.Create());
    }
}
