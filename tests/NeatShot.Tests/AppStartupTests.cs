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
    public void NativeTrayIcon_CreationAndDisposal_ShouldSucceedOnStaThread()
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
                using var tray = new NeatShot.Presentation.Tray.NativeTrayIcon(icon.Handle, "Test Tooltip");
                Assert.NotNull(tray);
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
    public void OverlayWindow_EyedropperMode_TogglesLoupeVisibility_OnStaThread()
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
                Assert.Equal(System.Windows.Visibility.Collapsed, window.EyedropperLoupe.Visibility);

                window.EnterEyedropperMode();
                Assert.Equal(System.Windows.Visibility.Visible, window.EyedropperLoupe.Visibility);
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.Eyedropper, window.DrawingControl.CurrentTool);

                window.ExitEyedropperMode();
                Assert.Equal(System.Windows.Visibility.Collapsed, window.EyedropperLoupe.Visibility);
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
