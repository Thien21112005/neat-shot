using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
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
    private TaskbarIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        InitializeTrayIcon();
        InitializeGlobalHotkeys();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IExportService, ExportService>();

        // ViewModels
        services.AddTransient<OverlayViewModel>();

        // Views
        services.AddTransient<OverlayWindow>();
    }

    private void InitializeTrayIcon()
    {
        var contextMenu = new ContextMenu();

        var captureItem = new MenuItem { Header = "Chụp vùng chọn (PrtSc)" };
        captureItem.Click += (s, e) => TriggerCapture();
        contextMenu.Items.Add(captureItem);

        var fullscreenItem = new MenuItem { Header = "Chụp toàn màn hình" };
        fullscreenItem.Click += (s, e) => TriggerCapture();
        contextMenu.Items.Add(fullscreenItem);

        contextMenu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "Thoát" };
        exitItem.Click += (s, e) => Shutdown();
        contextMenu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "NeatShot - Chụp màn hình thông minh",
            ContextMenu = contextMenu
        };

        // Click chuột trái vào tray icon để chụp
        _trayIcon.TrayLeftMouseDown += (s, e) => TriggerCapture();
    }

    private void InitializeGlobalHotkeys()
    {
        var hotkeyService = Services.GetRequiredService<IHotkeyService>();
        hotkeyService.HotkeyPressed += (s, e) => TriggerCapture();

        // Mặc định đăng ký phím PrintScreen
        var registered = hotkeyService.Register(Key.PrintScreen, ModifierKeys.None);
        if (!registered)
        {
            // Dự phòng đăng ký Ctrl+Shift+A nếu PrintScreen bị chiếm bởi ứng dụng khác
            hotkeyService.Register(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
        }
    }

    private async void TriggerCapture()
    {
        try
        {
            var captureService = Services.GetRequiredService<IScreenCaptureService>();
            var capturedScreen = await captureService.CaptureCleanScreenAsync();
            var virtualBounds = captureService.GetVirtualScreenBounds();

            var overlayWindow = Services.GetRequiredService<OverlayWindow>();
            overlayWindow.Display(virtualBounds, capturedScreen);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi chụp màn hình: {ex.Message}");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();

        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
