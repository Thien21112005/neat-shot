using Microsoft.Win32;
using NeatShot.Core.Services.Implementations;
using Xunit;

namespace NeatShot.Tests.Services;

public class StartupServiceTests : IDisposable
{
    // Key riêng cho test (HKCU), không đụng vào Run key thật của Windows
    private readonly string _runKeyPath = $@"Software\NeatShot.Tests\Run-{Guid.NewGuid():N}";

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(@"Software\NeatShot.Tests", throwOnMissingSubKey: false);
    }

    [Fact]
    public void Enable_RegistersQuotedExePathWithStartupFlag()
    {
        var service = new WindowsStartupService(_runKeyPath);

        service.Enable(@"C:\Apps\NeatShot\NeatShot.exe");

        Assert.True(service.IsEnabled());
        using var key = Registry.CurrentUser.OpenSubKey(_runKeyPath);
        Assert.Equal("\"C:\\Apps\\NeatShot\\NeatShot.exe\" --startup", key?.GetValue("NeatShot"));
    }

    [Fact]
    public void Enable_UpdatesThePath_WhenTheAppWasMoved()
    {
        var service = new WindowsStartupService(_runKeyPath);
        service.Enable(@"C:\Old\NeatShot.exe");

        service.Enable(@"D:\New\NeatShot.exe");

        using var key = Registry.CurrentUser.OpenSubKey(_runKeyPath);
        Assert.Equal("\"D:\\New\\NeatShot.exe\" --startup", key?.GetValue("NeatShot"));
    }

    [Fact]
    public void Disable_RemovesTheEntry_AndIsSafeToCallTwice()
    {
        var service = new WindowsStartupService(_runKeyPath);
        service.Enable(@"C:\Apps\NeatShot.exe");

        service.Disable();
        service.Disable();

        Assert.False(service.IsEnabled());
    }

    [Theory]
    [InlineData(null, false, true)]   // Lần đầu chạy bản phát hành: bật sẵn như Lightshot
    [InlineData(null, true, false)]   // Lần đầu chạy bản Debug (dev): không tự đăng ký
    [InlineData(true, true, true)]    // Người dùng đã chọn bật
    [InlineData(false, false, false)] // Người dùng đã chọn tắt
    public void ShouldLaunchAtStartup_ResolvesUserChoiceAndDefault(bool? userChoice, bool isDebugBuild, bool expected)
    {
        Assert.Equal(expected, WindowsStartupService.ShouldLaunchAtStartup(userChoice, isDebugBuild));
    }
}
