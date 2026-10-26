using Microsoft.Extensions.DependencyInjection;
using NeatShot.Common.Helpers;
using NeatShot.Common.Localization;
using NeatShot.Core.Interop;
using NeatShot.Core.Models;
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

        var settings = Services.GetRequiredService<ISettingsService>().LoadSettings();
        Loc.SetLanguage(settings.Language);
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
        services.AddSingleton<IStartupService>(_ => StartupServiceFactory.Create());
        services.AddSingleton<IAppHotkeyManager, AppHotkeyManager>();
        services.AddSingleton<IWindowRegionService, WindowRegionService>();

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

    private TrayMenuController? _trayMenuController;

    private void InitializeTrayIcon()
    {
        _iconHolder = TrayIconHelper.CreateTrayIcon();
        _trayIcon = new NativeTrayIcon(_iconHolder.Handle, Loc.T("Tray.Tooltip"));
        _trayIcon.Click += (s, e) => TriggerCapture();
        _trayIcon.ContextMenuRequested += (s, e) => _trayMenuController?.Open();

        BuildTrayMenu();

        // Đổi ngôn ngữ trong Cài đặt: dựng lại menu và tooltip của icon khay
        Loc.LanguageChanged += (s, e) =>
        {
            BuildTrayMenu();
            _trayIcon?.SetTooltip(Loc.T("Tray.Tooltip"));
        };
    }

    /// <summary>
    /// Dựng menu chuột phải của icon khay theo ngôn ngữ hiện tại.
    /// </summary>
    private void BuildTrayMenu()
    {
        _trayContextMenu = new ContextMenu();

        var captureItem = new MenuItem { Header = Loc.T("Tray.Capture") };
        captureItem.Click += (s, e) => TriggerCapture();
        _trayContextMenu.Items.Add(captureItem);

        var fullscreenItem = new MenuItem { Header = Loc.T("Tray.CaptureFullScreen") };
        fullscreenItem.Click += (s, e) => TriggerCapture(selectFullScreen: true);
        _trayContextMenu.Items.Add(fullscreenItem);

        var repeatItem = new MenuItem { Header = Loc.T("Tray.RepeatLast") };
        repeatItem.Click += (s, e) => TriggerCapture(repeatLastRegion: true);
        _trayContextMenu.Items.Add(repeatItem);

        var delayItem = new MenuItem { Header = Loc.T("Tray.Delayed") };
        foreach (var seconds in new[] { 3, 5, 10 })
        {
            var item = new MenuItem { Header = Loc.F("Tray.DelaySeconds", seconds) };
            item.Click += (s, e) => StartDelayedCapture(seconds);
            delayItem.Items.Add(item);
        }
        _trayContextMenu.Items.Add(delayItem);

        var pinClipboardItem = new MenuItem { Header = Loc.T("Tray.PinClipboard") };
        pinClipboardItem.Click += (s, e) => PinFromClipboard();
        _trayContextMenu.Items.Add(pinClipboardItem);

        _trayContextMenu.Items.Add(new Separator());

        var galleryItem = new MenuItem { Header = Loc.T("Tray.Gallery") };
        galleryItem.Click += (s, e) => OpenGallery();
        _trayContextMenu.Items.Add(galleryItem);

        var folderItem = new MenuItem { Header = Loc.T("Tray.OpenFolder") };
        folderItem.Click += (s, e) => OpenScreenshotsFolder();
        _trayContextMenu.Items.Add(folderItem);

        _trayContextMenu.Items.Add(new Separator());

        var startupItem = new MenuItem
        {
            Header = Loc.T("Tray.StartWithWindows"),
            IsCheckable = true,
            IsChecked = Services.GetRequiredService<IStartupService>().IsEnabled()
        };
        startupItem.Click += (s, e) => SetLaunchAtStartup(startupItem.IsChecked);
        _trayContextMenu.Items.Add(startupItem);

        var settingsItem = new MenuItem { Header = Loc.T("Tray.Settings") };
        settingsItem.Click += (s, e) => OpenSettings();
        _trayContextMenu.Items.Add(settingsItem);

        _trayContextMenu.Items.Add(new Separator());

        // Menu luôn hiển thị phím tắt & trạng thái mới nhất (có thể vừa đổi trong cửa sổ Cài đặt)
        _trayContextMenu.Opened += (s, e) =>
        {
            var hotkeys = Services.GetRequiredService<IAppHotkeyManager>();
            captureItem.InputGestureText = hotkeys.GetActiveGesture(HotkeyAction.Capture)?.DisplayText ?? string.Empty;
            pinClipboardItem.InputGestureText = hotkeys.GetActiveGesture(HotkeyAction.PinFromClipboard)?.DisplayText ?? string.Empty;
            repeatItem.InputGestureText = hotkeys.GetActiveGesture(HotkeyAction.RepeatLastCapture)?.DisplayText ?? string.Empty;
            repeatItem.IsEnabled = Services.GetRequiredService<ISettingsService>().CurrentSettings.LastCaptureRegion != null;
            startupItem.IsChecked = Services.GetRequiredService<IStartupService>().IsEnabled();
        };

        var exitItem = new MenuItem { Header = Loc.T("Tray.Exit") };
        exitItem.Click += (s, e) => Shutdown();
        _trayContextMenu.Items.Add(exitItem);

        _trayMenuController = new TrayMenuController(_trayContextMenu);
    }

    /// <summary>
    /// Đăng ký phím tắt toàn cục theo cài đặt của người dùng (đổi được trong cửa sổ Cài đặt, có hiệu lực ngay).
    /// </summary>
    private string InitializeGlobalHotkeys()
    {
        var hotkeys = Services.GetRequiredService<IAppHotkeyManager>();
        hotkeys.SetHandler(HotkeyAction.Capture, () => TriggerCapture());
        hotkeys.SetHandler(HotkeyAction.CaptureAlternate, () => TriggerCapture());
        hotkeys.SetHandler(HotkeyAction.PinFromClipboard, PinFromClipboard);
        hotkeys.SetHandler(HotkeyAction.RepeatLastCapture, () => TriggerCapture(repeatLastRegion: true));
        hotkeys.Apply(Services.GetRequiredService<ISettingsService>().CurrentSettings);

        var activeKeys = new[] { HotkeyAction.Capture, HotkeyAction.CaptureAlternate }
            .Select(action => hotkeys.GetActiveGesture(action)?.DisplayText)
            .Where(text => text != null)
            .ToList();

        return activeKeys.Count > 0 ? string.Join(" hoặc ", activeKeys) : "Chưa đăng ký (bị chiếm dụng)";
    }

    /// <summary>
    /// Ghim ảnh đang có trong Clipboard (copy từ web, Zalo, Figma, Explorer…) thành cửa sổ nổi.
    /// </summary>
    private static void PinFromClipboard()
    {
        try
        {
            var image = ClipboardImageReader.ReadFromClipboard();
            if (image == null)
            {
                ToastWindow.Show(Loc.T("Toast.ClipboardNoImage"), Loc.T("Toast.ClipboardNoImageHint"), ToastKind.Warning);
                return;
            }

            var screen = new System.Windows.Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            var pinWindow = new PinWindow(new PinViewModel());
            pinWindow.SetImage(image, GalleryWindow.CalculatePinPosition(image, screen));
            pinWindow.Show();
            pinWindow.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Lỗi ghim ảnh từ Clipboard: {ex.Message}");
        }
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
            _activeSettingsWindow.Closed += (s, e) =>
            {
                _activeSettingsWindow = null;
                // Ngôn ngữ vừa lưu có hiệu lực ngay cho menu khay và các cửa sổ mở sau đó
                Loc.SetLanguage(Services.GetRequiredService<ISettingsService>().CurrentSettings.Language);
            };
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
            ToastWindow.Show(Loc.T("Toast.OpenFolderFailed"), ex.Message, ToastKind.Warning);
        }
    }

    /// <summary>
    /// Báo đã sao chép/lưu ảnh kèm thư mục đã lưu. Kể cả khi Windows chặn thư mục đã chọn và ảnh được lưu
    /// sang thư mục dự phòng, vẫn chỉ báo thành công (không cảnh báo về bảo mật làm người dùng hoang mang).
    /// </summary>
    private static void NotifyCaptureStored(string message, string? savedPath)
    {
        ToastWindow.Show(message, ToastWindow.DescribeSavedLocation(savedPath));
    }

    private CountdownWindow? _activeCountdown;

    /// <summary>
    /// Chụp hẹn giờ: đếm ngược để người dùng kịp mở menu/tooltip, rồi chụp mà KHÔNG đóng popup.
    /// </summary>
    private void StartDelayedCapture(int seconds)
    {
        if (_activeCountdown != null || (_activeOverlayWindow != null && _activeOverlayWindow.IsLoaded)) return;

        if (_trayContextMenu != null && _trayContextMenu.IsOpen)
        {
            _trayContextMenu.IsOpen = false;
        }

        var countdown = new CaptureCountdown(seconds);
        var window = new CountdownWindow(countdown);
        _activeCountdown = window;

        countdown.Cancelled += (s, e) =>
        {
            window.Close();
            _activeCountdown = null;
        };
        countdown.Completed += async (s, e) =>
        {
            window.Close();
            _activeCountdown = null;

            // Chờ cửa sổ đếm ngược biến mất hẳn khỏi màn hình trước khi chụp
            await Task.Delay(150);
            TriggerCapture(dismissPopups: false);
        };

        window.ShowAndStart();
    }

    private async void TriggerCapture(bool selectFullScreen = false, bool dismissPopups = true, bool repeatLastRegion = false)
    {
        try
        {
            if (_activeOverlayWindow != null && _activeOverlayWindow.IsLoaded)
            {
                _activeOverlayWindow.Activate();
                return;
            }

            // Đóng context menu khay hệ thống nếu đang mở (trừ khi chụp hẹn giờ: người dùng muốn giữ menu của họ)
            if (dismissPopups && _trayContextMenu != null && _trayContextMenu.IsOpen)
            {
                _trayContextMenu.IsOpen = false;
            }

            var captureService = Services.GetRequiredService<IScreenCaptureService>();
            var capturedScreen = await captureService.CaptureScreenAsync(dismissPopups);
            var virtualBounds = captureService.GetVirtualScreenBounds();
            var windowRegions = DetectWindowRegions(capturedScreen);

            _activeOverlayWindow = Services.GetRequiredService<OverlayWindow>();
            _activeOverlayWindow.Closed += (s, e) => _activeOverlayWindow = null;
            _activeOverlayWindow.CaptureCopied += (s, savedPath) => NotifyCaptureStored(Loc.T("Toast.Copied"), savedPath);
            _activeOverlayWindow.CaptureSaved += (s, savedPath) => NotifyCaptureStored(Loc.T("Toast.Saved"), savedPath);
            var preselect = repeatLastRegion
                ? Services.GetRequiredService<ISettingsService>().CurrentSettings.LastCaptureRegion
                : null;
            _activeOverlayWindow.Display(virtualBounds, capturedScreen, selectFullScreen, windowRegions, preselect);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NeatShot] Lỗi chụp màn hình: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Lỗi chụp màn hình: {ex.Message}");
        }
    }

    /// <summary>
    /// Liệt kê cửa sổ đang mở (trước khi overlay che màn hình) để rê chuột là viền sẵn cửa sổ.
    /// </summary>
    private static IReadOnlyList<Core.Models.WindowRegion> DetectWindowRegions(System.Windows.Media.Imaging.BitmapSource capturedScreen)
    {
        try
        {
            var scaleX = capturedScreen.DpiX > 0 ? capturedScreen.DpiX / 96.0 : 1.0;
            var scaleY = capturedScreen.DpiY > 0 ? capturedScreen.DpiY / 96.0 : 1.0;
            return Services.GetRequiredService<IWindowRegionService>().GetOverlayRegions(scaleX, scaleY);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Không liệt kê được cửa sổ: {ex.Message}");
            return Array.Empty<Core.Models.WindowRegion>();
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
