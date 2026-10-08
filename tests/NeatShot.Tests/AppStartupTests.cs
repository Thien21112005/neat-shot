using Microsoft.Extensions.DependencyInjection;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using System.Windows;
using System.Windows.Input;
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
        services.AddSingleton<IOcrService, WindowsOcrService>();
        services.AddSingleton<IBeautifyService, BeautifyService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<PinViewModel>();
        services.AddTransient<OverlayWindow>();
        services.AddTransient<PinWindow>();

        var provider = services.BuildServiceProvider();

        // Act & Assert
        Assert.NotNull(provider.GetRequiredService<IScreenCaptureService>());
        Assert.NotNull(provider.GetRequiredService<IHotkeyService>());
        Assert.NotNull(provider.GetRequiredService<IExportService>());
        Assert.NotNull(provider.GetRequiredService<IOcrService>());
        Assert.NotNull(provider.GetRequiredService<IBeautifyService>());
        Assert.NotNull(provider.GetRequiredService<ISettingsService>());
        Assert.NotNull(provider.GetRequiredService<OverlayViewModel>());
        Assert.NotNull(provider.GetRequiredService<PinViewModel>());
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

    [Fact]
    public void OverlayWindow_KeyboardShortcuts_SwitchToolsAndDeleteSelected_OnStaThread()
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

                // 1. Tool shortcut: T -> Text
                Assert.True(window.ProcessShortcut(Key.T, ModifierKeys.None));
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.Text, window.DrawingControl.CurrentTool);

                // 2. Tool shortcut: H -> Highlight
                Assert.True(window.ProcessShortcut(Key.H, ModifierKeys.None));
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.Highlight, window.DrawingControl.CurrentTool);

                // 3. Tool shortcut: V -> Select
                Assert.True(window.ProcessShortcut(Key.V, ModifierKeys.None));
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.Select, window.DrawingControl.CurrentTool);

                // 4. Tool shortcut: I -> Eyedropper & Loupe visible
                Assert.True(window.ProcessShortcut(Key.I, ModifierKeys.None));
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.Eyedropper, window.DrawingControl.CurrentTool);
                Assert.Equal(Visibility.Visible, window.EyedropperLoupe.Visibility);

                // 5. Esc exits Eyedropper mode
                Assert.True(window.ProcessShortcut(Key.Escape, ModifierKeys.None));
                Assert.Equal(Visibility.Collapsed, window.EyedropperLoupe.Visibility);

                // 6. Delete shortcut deletes selected element
                var elem = new NeatShot.Core.Models.DrawingElement { ToolType = NeatShot.Core.Models.DrawingToolType.Rectangle, Rect = new Rect(0, 0, 50, 50) };
                window.DrawingControl.UndoStack.Push(elem);
                window.DrawingControl.SelectElementAt(new Point(25, 25));
                Assert.NotNull(window.DrawingControl.SelectedElement);

                Assert.True(window.ProcessShortcut(Key.Delete, ModifierKeys.None));
                Assert.Null(window.DrawingControl.SelectedElement);
                Assert.Empty(window.DrawingControl.UndoStack.Items);

                // 7. Esc with active tool (Pencil) resets tool to None instead of closing window
                window.DrawingControl.CurrentTool = NeatShot.Core.Models.DrawingToolType.Pencil;
                Assert.True(window.ProcessShortcut(Key.Escape, ModifierKeys.None));
                Assert.Equal(NeatShot.Core.Models.DrawingToolType.None, window.DrawingControl.CurrentTool);
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
    public void OverlayWindow_InitializesColorHistory_FromSettingsService_OnStaThread()
    {
        var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "neatshot_test_settings_" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var settingsService = new SettingsService(tempFile);
            settingsService.SaveSettings(new NeatShot.Core.Models.AppSettings
            {
                RecentColorsHex = new List<string> { "#FFFF0000", "#FF00FF00" }
            });

            var services = new ServiceCollection();
            services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<IOcrService, WindowsOcrService>();
            services.AddSingleton<IBeautifyService, BeautifyService>();
            services.AddSingleton<ISettingsService>(settingsService);
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
                    var history = window.Toolbar.GetColorHistoryHex();
                    Assert.Equal(2, history.Count);
                    Assert.Equal("#FFFF0000", history[0]);
                    Assert.Equal("#FF00FF00", history[1]);
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
        finally
        {
            if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile);
        }
    }
}
