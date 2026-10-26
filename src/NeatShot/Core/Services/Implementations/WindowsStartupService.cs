using Microsoft.Win32;
using NeatShot.Core.Services.Interfaces;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Đăng ký NeatShot vào HKCU\...\CurrentVersion\Run để tự chạy nền khi đăng nhập Windows
/// (không cần quyền admin; người dùng có thể bật/tắt trong Task Manager → Startup apps).
/// </summary>
public class WindowsStartupService : IStartupService
{
    public const string DefaultRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string StartupArgument = "--startup";
    private const string ValueName = "NeatShot";

    private readonly string _runKeyPath;

    public WindowsStartupService() : this(DefaultRunKeyPath)
    {
    }

    public WindowsStartupService(string runKeyPath)
    {
        _runKeyPath = string.IsNullOrWhiteSpace(runKeyPath) ? DefaultRunKeyPath : runKeyPath;
    }

    /// <summary>
    /// Quyết định có nên chạy cùng Windows: theo lựa chọn của người dùng nếu đã có;
    /// lần đầu thì bật sẵn (giống Lightshot), trừ bản Debug để không đăng ký file exe trong bin\Debug.
    /// </summary>
    public static bool ShouldLaunchAtStartup(bool? userChoice, bool isDebugBuild) => userChoice ?? !isDebugBuild;

    /// <inheritdoc />
    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(_runKeyPath);
        return key?.GetValue(ValueName) is string command && !string.IsNullOrWhiteSpace(command);
    }

    /// <inheritdoc />
    public void Enable(string exePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exePath);

        using var key = Registry.CurrentUser.CreateSubKey(_runKeyPath, writable: true);
        key.SetValue(ValueName, $"\"{exePath}\" {StartupArgument}", RegistryValueKind.String);
    }

    /// <inheritdoc />
    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
