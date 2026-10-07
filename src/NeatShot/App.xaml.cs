using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using NeatShot.Common.Helpers;
using NeatShot.Core.Interop;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
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
    private TaskbarIcon? _trayIcon;
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
        Console.WriteLine("  Thao tác: Bấm phím tắt hoặc click vào icon NeatShot dưới khay hệ thống.");
        Console.WriteLine("==================================================");
        Console.WriteLine();
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
            ToolTipText = "NeatShot - Chụp màn hình thông minh (Click để chụp)",
            IconSource = TrayIconHelper.CreateTrayIcon(),
            ContextMenu = contextMenu
        };

        // Click chuột trái vào tray icon để chụp
        _trayIcon.TrayLeftMouseDown += (s, e) => TriggerCapture();
    }

    private string InitializeGlobalHotkeys()
    {
        var hotkeyService = Services.GetRequiredService<IHotkeyService>();
        hotkeyService.HotkeyPressed += (s, e) => TriggerCapture();

        // Mặc định đăng ký phím PrintScreen
        var registered = hotkeyService.Register(Key.PrintScreen, ModifierKeys.None);
        if (registered)
        {
            return "PrintScreen";
        }

        // Dự phòng đăng ký Ctrl+Shift+A nếu PrintScreen bị chiếm bởi ứng dụng khác
        registered = hotkeyService.Register(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
        return registered ? "Ctrl + Shift + A" : "Chưa đăng ký (bị chiếm dụng)";
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
            Console.WriteLine($"[NeatShot] Lỗi chụp màn hình: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Lỗi chụp màn hình: {ex.Message}");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();

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
