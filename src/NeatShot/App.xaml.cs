using Microsoft.Extensions.DependencyInjection;
using NeatShot.Common.Helpers;
using NeatShot.Core.Interop;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.Tray;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeatShot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private NativeTrayIcon? _trayIcon;
    private Icon? _iconHolder;
    private ContextMenu? _trayContextMenu;
    private static Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Đính kèm Console nếu chạy từ Terminal để cung cấp phản hồi cho người dùng
        NativeMethods.AttachConsole(NativeConstants.ATTACH_PARENT_PROCESS);

        _singleInstanceMutex = new Mutex(true, "Global\\NeatShot_SingleInstance_Mutex", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Console.WriteLine("[NeatShot] Ứng dụng đã đang chạy ngầm trong khay hệ thống (System Tray).");
            Shutdown();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        Services.GetRequiredService<ISettingsService>().LoadSettings();
        ApplyStartupPreference();

        InitializeTrayIcon();
        var activeHotkey = InitializeGlobalHotkeys();

        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("  NeatShot - Chụp màn hình thông minh");
        Console.WriteLine("  Trạng thái: Đang chạy ngầm trong khay hệ thống (System Tray)");
        Console.WriteLine($"  Phím tắt chụp: {activeHotkey}");
        Console.WriteLine("  * Mẹo: Đổi phím tắt trong menu khay → Cài đặt…");
        Console.WriteLine("==================================================");
        Console.WriteLine();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IOcrService, WindowsOcrService>();
        services.AddSingleton<IBeautifyService, BeautifyService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IScreenshotGalleryService, ScreenshotGalleryService>();
        services.AddSingleton<IStartupService, WindowsStartupService>();
        services.AddSingleton<IAppHotkeyManager, AppHotkeyManager>();

        // ViewModels
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<PinViewModel>();
        services.AddTransient<GalleryViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<OverlayWindow>();
        services.AddTransient<PinWindow>();
        services.AddTransient<GalleryWindow>();
        services.AddTransient<SettingsWindow>();
    }

    private void InitializeTrayIcon()
    {
        _trayContextMenu = new ContextMenu();

        var captureItem = new MenuItem { Header = "Chụp vùng chọn" };
        captureItem.Click += (s, e) => TriggerCapture();
        _trayContextMenu.Items.Add(captureItem);

        var fullscreenItem = new MenuItem { Header = "Chụp toàn màn hình" };
        fullscreenItem.Click += (s, e) => TriggerCapture(selectFullScreen: true);
        _trayContextMenu.Items.Add(fullscreenItem);

        _trayContextMenu.Items.Add(new Separator());

        var galleryItem = new MenuItem { Header = "🖼️ Thư viện ảnh chụp" };
        galleryItem.Click += (s, e) => OpenGallery();
        _trayContextMenu.Items.Add(galleryItem);

        var folderItem = new MenuItem { Header = "📁 Mở thư mục ảnh NeatShot" };
        folderItem.Click += (s, e) => OpenScreenshotsFolder();
        _trayContextMenu.Items.Add(folderItem);

        _trayContextMenu.Items.Add(new Separator());

        var startupItem = new MenuItem
        {
            Header = "Khởi động cùng Windows",
            IsCheckable = true,
            IsChecked = Services.GetRequiredService<IStartupService>().IsEnabled()
        };
        startupItem.Click += (s, e) => SetLaunchAtStartup(startupItem.IsChecked);
        _trayContextMenu.Items.Add(startupItem);

        var settingsItem = new MenuItem { Header = "⚙️ Cài đặt…" };
        settingsItem.Click += (s, e) => OpenSettings();
        _trayContextMenu.Items.Add(settingsItem);

        _trayContextMenu.Items.Add(new Separator());

        // Menu luôn hiển thị phím tắt & trạng thái mới nhất (có thể vừa đổi trong cửa sổ Cài đặt)
        _trayContextMenu.Opened += (s, e) =>
        {
            var hotkeys = Services.GetRequiredService<IAppHotkeyManager>();
            captureItem.InputGestureText = hotkeys.GetActiveGesture(HotkeyAction.Capture)?.DisplayText ?? string.Empty;
            startupItem.IsChecked = Services.GetRequiredService<IStartupService>().IsEnabled();
        };

        var exitItem = new MenuItem { Header = "Thoát" };
        exitItem.Click += (s, e) => Shutdown();
        _trayContextMenu.Items.Add(exitItem);

        _iconHolder = TrayIconHelper.CreateTrayIcon();
        _trayIcon = new NativeTrayIcon(_iconHolder.Handle, "NeatShot - Chụp màn hình thông minh (Click để chụp)");

        _trayIcon.Click += (s, e) => TriggerCapture();
        _trayIcon.ContextMenuRequested += (s, e) =>
        {
            if (_trayContextMenu != null)
            {
                _trayContextMenu.IsOpen = true;
            }
        };
    }

    /// <summary>
    /// Đăng ký phím tắt toàn cục theo cài đặt của người dùng (đổi được trong cửa sổ Cài đặt, có hiệu lực ngay).
    /// </summary>
    private string InitializeGlobalHotkeys()
    {
        var hotkeys = Services.GetRequiredService<IAppHotkeyManager>();
        hotkeys.SetHandler(HotkeyAction.Capture, () => TriggerCapture());
        hotkeys.SetHandler(HotkeyAction.CaptureAlternate, () => TriggerCapture());
        hotkeys.Apply(Services.GetRequiredService<ISettingsService>().CurrentSettings);

        var activeKeys = new[] { HotkeyAction.Capture, HotkeyAction.CaptureAlternate }
            .Select(action => hotkeys.GetActiveGesture(action)?.DisplayText)
            .Where(text => text != null)
            .ToList();

        return activeKeys.Count > 0 ? string.Join(" hoặc ", activeKeys) : "Chưa đăng ký (bị chiếm dụng)";
    }

    private SettingsWindow? _activeSettingsWindow;

    private void OpenSettings()
    {
        try
        {
            if (_activeSettingsWindow != null && _activeSettingsWindow.IsLoaded)
            {
                _activeSettingsWindow.Activate();
                return;
            }

            _activeSettingsWindow = Services.GetRequiredService<SettingsWindow>();
            _activeSettingsWindow.Closed += (s, e) => _activeSettingsWindow = null;
            _activeSettingsWindow.Show();
            _activeSettingsWindow.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Lỗi mở cửa sổ cài đặt: {ex.Message}");
        }
    }

#if DEBUG
    private const bool IsDebugBuild = true;
#else
    private const bool IsDebugBuild = false;
#endif

    /// <summary>
    /// Đồng bộ đăng ký khởi động cùng Windows với lựa chọn của người dùng.
    /// Lần đầu chạy bản phát hành sẽ tự bật (giống Lightshot); mỗi lần mở app đều cập nhật lại đường dẫn exe.
    /// </summary>
    private static void ApplyStartupPreference()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return;

        try
        {
            var startup = Services.GetRequiredService<IStartupService>();
            var choice = Services.GetRequiredService<ISettingsService>().CurrentSettings.LaunchAtStartup;

            if (choice == false)
            {
                startup.Disable();
            }
            else if (WindowsStartupService.ShouldLaunchAtStartup(choice, IsDebugBuild))
            {
                startup.Enable(exePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Không thể cập nhật khởi động cùng Windows: {ex.Message}");
        }
    }

    /// <summary>
    /// Người dùng bật/tắt "Khởi động cùng Windows" từ menu khay hệ thống.
    /// </summary>
    private static void SetLaunchAtStartup(bool enabled)
    {
        var exePath = Environment.ProcessPath;
        try
        {
            var startup = Services.GetRequiredService<IStartupService>();
            if (enabled && !string.IsNullOrEmpty(exePath))
            {
                startup.Enable(exePath);
            }
            else
            {
                startup.Disable();
            }

            var settingsService = Services.GetRequiredService<ISettingsService>();
            settingsService.CurrentSettings.LaunchAtStartup = enabled;
            settingsService.SaveSettings(settingsService.CurrentSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Không thể đổi khởi động cùng Windows: {ex.Message}");
        }
    }

    private OverlayWindow? _activeOverlayWindow;
    private GalleryWindow? _activeGalleryWindow;

    private void OpenGallery()
    {
        try
        {
            if (_activeGalleryWindow != null && _activeGalleryWindow.IsLoaded)
            {
                _activeGalleryWindow.Activate();
                if (_activeGalleryWindow.WindowState == WindowState.Minimized)
                {
                    _activeGalleryWindow.WindowState = WindowState.Normal;
                }
                return;
            }

            _activeGalleryWindow = Services.GetRequiredService<GalleryWindow>();
            _activeGalleryWindow.Closed += (s, e) => _activeGalleryWindow = null;
            _activeGalleryWindow.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Lỗi mở thư viện ảnh: {ex.Message}");
        }
    }

    private void OpenScreenshotsFolder()
    {
        try
        {
            var galleryService = Services.GetRequiredService<IScreenshotGalleryService>();
            var dir = galleryService.GetScreenshotsDirectory();
            if (System.IO.Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Lỗi mở thư mục ảnh: {ex.Message}");
        }
    }

    private async void TriggerCapture(bool selectFullScreen = false)
    {
        try
        {
            if (_activeOverlayWindow != null && _activeOverlayWindow.IsLoaded)
            {
                _activeOverlayWindow.Activate();
                return;
            }

            // Đóng context menu khay hệ thống nếu đang mở
            if (_trayContextMenu != null && _trayContextMenu.IsOpen)
            {
                _trayContextMenu.IsOpen = false;
            }

            var captureService = Services.GetRequiredService<IScreenCaptureService>();
            var capturedScreen = await captureService.CaptureCleanScreenAsync();
            var virtualBounds = captureService.GetVirtualScreenBounds();

            _activeOverlayWindow = Services.GetRequiredService<OverlayWindow>();
            _activeOverlayWindow.Closed += (s, e) => _activeOverlayWindow = null;
            _activeOverlayWindow.CaptureCopied += (s, savedPath) => ToastWindow.ShowCopied(savedPath);
            _activeOverlayWindow.Display(virtualBounds, capturedScreen, selectFullScreen);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NeatShot] Lỗi chụp màn hình: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Lỗi chụp màn hình: {ex.Message}");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _iconHolder?.Dispose();

        if (_singleInstanceMutex != null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch
            {
                // Bỏ qua nếu mutex không được sở hữu
            }
            _singleInstanceMutex.Dispose();
        }

        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
