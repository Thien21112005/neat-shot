using NeatShot.Core.Services.Interfaces;
using Windows.ApplicationModel;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Lớp mỏng bọc <see cref="StartupTask"/> của Windows để có thể thay bằng bản giả khi test.
/// </summary>
public interface IStartupTaskHandle
{
    StartupTaskState State { get; }

    /// <summary>Xin Windows bật tác vụ khởi động; trả về trạng thái sau khi xin.</summary>
    StartupTaskState RequestEnable();

    void Disable();
}

/// <summary>
/// Tự khởi động cùng Windows khi NeatShot chạy từ gói MSIX (Microsoft Store).
/// App trong gói không được dùng khóa Run trong registry, mà phải dùng StartupTask khai báo trong manifest
/// (packaging/msix/AppxManifest.xml, TaskId <see cref="TaskId"/>). Người dùng quản lý được trong Settings → Apps → Startup.
/// </summary>
public class PackagedStartupService : IStartupService
{
    public const string TaskId = "NeatShotStartup";

    private readonly IStartupTaskHandle _task;

    public PackagedStartupService(IStartupTaskHandle task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
    }

    /// <inheritdoc />
    public bool IsEnabled() => _task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;

    /// <inheritdoc />
    /// <remarks>Đường dẫn exe không cần: Windows tự chạy app trong gói.</remarks>
    public void Enable(string exePath)
    {
        if (IsEnabled()) return;

        if (_task.State == StartupTaskState.DisabledByUser)
        {
            throw new InvalidOperationException(
                "Bạn đã tắt NeatShot trong Settings → Apps → Startup của Windows; hãy bật lại ở đó.");
        }

        var result = _task.RequestEnable();
        if (result is not (StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy))
        {
            throw new InvalidOperationException(
                $"Windows không cho phép tự khởi động ({result}). Kiểm tra Settings → Apps → Startup.");
        }
    }

    /// <inheritdoc />
    public void Disable()
    {
        if (IsEnabled()) _task.Disable();
    }
}

/// <summary>
/// <see cref="IStartupTaskHandle"/> thật, gọi API WinRT. Các lời gọi bất đồng bộ được chạy trên thread pool
/// để không chặn luồng UI chờ chính nó.
/// </summary>
public sealed class WinRtStartupTask : IStartupTaskHandle
{
    private readonly Lazy<StartupTask> _task;

    public WinRtStartupTask(string taskId)
    {
        _task = new Lazy<StartupTask>(() => Task.Run(async () => await StartupTask.GetAsync(taskId)).GetAwaiter().GetResult());
    }

    public StartupTaskState State => _task.Value.State;

    public StartupTaskState RequestEnable() =>
        Task.Run(async () => await _task.Value.RequestEnableAsync()).GetAwaiter().GetResult();

    public void Disable() => _task.Value.Disable();
}

/// <summary>
/// Chọn cách tự khởi động phù hợp: StartupTask khi chạy từ gói MSIX, khóa Run khi chạy bản portable.
/// </summary>
public static class StartupServiceFactory
{
    public static IStartupService Create() => Common.Helpers.AppPackage.IsPackaged
        ? new PackagedStartupService(new WinRtStartupTask(PackagedStartupService.TaskId))
        : new WindowsStartupService();
}
