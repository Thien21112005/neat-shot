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

        InitializeTrayIcon();
        var activeHotkey = InitializeGlobalHotkeys();

        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("  NeatShot - Chụp màn hình thông minh");
        Console.WriteLine("  Trạng thái: Đang chạy ngầm trong khay hệ thống (System Tray)");
        Console.WriteLine($"  Phím tắt chụp: {activeHotkey}");
        Console.WriteLine("  * Mẹo: Nếu phím PrintScreen bị Windows Snipping Tool chặn,");
        Console.WriteLine("    hãy nhấn Ctrl + Shift + A hoặc click icon NeatShot ở khay hệ thống.");
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

        // ViewModels
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<PinViewModel>();

        // Views
        services.AddTransient<OverlayWindow>();
        services.AddTransient<PinWindow>();
    }

    private void InitializeTrayIcon()
    {
        _trayContextMenu = new ContextMenu();

        var captureItem = new MenuItem { Header = "Chụp vùng chọn (Ctrl+Shift+A / PrtSc)" };
        captureItem.Click += (s, e) => TriggerCapture();
        _trayContextMenu.Items.Add(captureItem);

        var fullscreenItem = new MenuItem { Header = "Chụp toàn màn hình" };
        fullscreenItem.Click += (s, e) => TriggerCapture();
        _trayContextMenu.Items.Add(fullscreenItem);

        _trayContextMenu.Items.Add(new Separator());

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

    private string InitializeGlobalHotkeys()
    {
        var hotkeyService = Services.GetRequiredService<IHotkeyService>();
        hotkeyService.HotkeyPressed += (s, e) => TriggerCapture();

        var activeKeys = new List<string>();

        // 1. Đăng ký phím tắt chính Ctrl + Shift + A (không bao giờ bị Windows 11 chặn)
        if (hotkeyService.Register(Key.A, ModifierKeys.Control | ModifierKeys.Shift))
        {
            activeKeys.Add("Ctrl + Shift + A");
        }

        // 2. Đăng ký phím PrintScreen truyền thống
        if (hotkeyService.Register(Key.PrintScreen, ModifierKeys.None))
        {
            activeKeys.Add("PrintScreen");
        }

        return activeKeys.Count > 0 ? string.Join(" hoặc ", activeKeys) : "Chưa đăng ký (bị chiếm dụng)";
    }

    private OverlayWindow? _activeOverlayWindow;

    private async void TriggerCapture()
    {
        try
        {
            if (_activeOverlayWindow != null && _activeOverlayWindow.IsLoaded)
            {
                _activeOverlayWindow.Activate();
                return;
            }

            var captureService = Services.GetRequiredService<IScreenCaptureService>();
            var capturedScreen = await captureService.CaptureCleanScreenAsync();
            var virtualBounds = captureService.GetVirtualScreenBounds();

            _activeOverlayWindow = Services.GetRequiredService<OverlayWindow>();
            _activeOverlayWindow.Closed += (s, e) => _activeOverlayWindow = null;
            _activeOverlayWindow.Display(virtualBounds, capturedScreen);
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
