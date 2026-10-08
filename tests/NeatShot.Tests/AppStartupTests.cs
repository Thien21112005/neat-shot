using Microsoft.Extensions.DependencyInjection;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using Xunit;

namespace NeatShot.Tests;

public class AppStartupTests
{
    [Fact]
    public void ServiceProvider_ShouldResolveAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<OverlayWindow>();

        var provider = services.BuildServiceProvider();

        // Act & Assert
        Assert.NotNull(provider.GetRequiredService<IScreenCaptureService>());
        Assert.NotNull(provider.GetRequiredService<IHotkeyService>());
        Assert.NotNull(provider.GetRequiredService<IExportService>());
        Assert.NotNull(provider.GetRequiredService<OverlayViewModel>());
    }

    [Fact]
    public void ServiceProvider_ShouldResolveOverlayWindow_OnStaThread()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<OverlayWindow>();

        var provider = services.BuildServiceProvider();

        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = provider.GetRequiredService<OverlayWindow>();
                Assert.NotNull(window);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException);
    }

    [Fact]
    public void TrayIconHelper_CreateTrayIcon_ShouldReturnValidIcon()
    {
        using var icon = NeatShot.Common.Helpers.TrayIconHelper.CreateTrayIcon();
        Assert.NotNull(icon);
        Assert.Equal(32, icon.Width);
        Assert.Equal(32, icon.Height);

        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                var trayIcon = new H.NotifyIcon.TaskbarIcon();
                trayIcon.Icon = icon;
                Assert.NotNull(trayIcon.Icon);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException);
    }
}
