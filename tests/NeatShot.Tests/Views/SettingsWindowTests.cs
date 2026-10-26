using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using NeatShot.Presentation.ViewModels;
using NeatShot.Presentation.Views;
using NeatShot.Tests.Fakes;
using System.IO;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using Xunit;

namespace NeatShot.Tests.Views;

public class SettingsWindowTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(Path.GetTempPath(), "NeatShot_SettingsWindow_" + Guid.NewGuid().ToString("N") + ".json");

    public void Dispose()
    {
        try { File.Delete(_settingsPath); } catch { /* bỏ qua */ }
    }

    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { threadException = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(threadException);
    }

    [Fact]
    public void SettingsWindow_ShowsCurrentValues_AndSuspendsHotkeysWhileRecording_OnStaThread()
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings { AutoCopyOnCapture = true, DefaultSaveDirectory = @"D:\Shots" });
        var hotkeys = new FakeHotkeyService();
        var manager = new AppHotkeyManager(hotkeys);
        manager.SetHandler(HotkeyAction.Capture, () => { });
        manager.Apply(settings.CurrentSettings);

        RunOnSta(() =>
        {
            var window = new SettingsWindow(new SettingsViewModel(settings, new FakeStartupService(), manager, () => @"C:\NeatShot.exe"));
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.DataBind);

            Assert.Equal(new HotkeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift), window.CaptureHotkeyBox.Gesture);
            Assert.Equal(@"D:\Shots", window.SaveDirectoryText.Text);
            Assert.True(window.AutoCopyToggle.IsChecked);
            Assert.True(window.AutoSaveToggle.IsChecked);

            window.CaptureHotkeyBox.StartRecording();
            Assert.Empty(hotkeys.Registered);

            window.CaptureHotkeyBox.HandleKey(Key.S, ModifierKeys.Control | ModifierKeys.Alt);
            Assert.Single(hotkeys.Registered);
            Assert.Equal(new HotkeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt), window.ViewModel.CaptureHotkey);
            window.Close();
        });
    }

    private static SettingsWindow CreateWindow(SettingsService settings)
    {
        var window = new SettingsWindow(new SettingsViewModel(settings, new FakeStartupService(), new AppHotkeyManager(new FakeHotkeyService()), () => @"C:\NeatShot.exe"));
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.DataBind);
        return window;
    }

    private static double Luminance(SettingsWindow window, string key)
    {
        var color = ((System.Windows.Media.SolidColorBrush)window.FindResource(key)).Color;
        return (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255;
    }

    [Theory]
    [InlineData("light", true)]
    [InlineData("dark", false)]
    public void SettingsWindow_UsesTheChosenTheme_OnStaThread(string theme, bool expectLight)
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings { Theme = theme });

        RunOnSta(() =>
        {
            var window = CreateWindow(settings);

            Assert.Equal(expectLight, Luminance(window, "StWindowBg") > 0.8);
            // Chữ chính luôn tương phản mạnh với nền
            Assert.True(Math.Abs(Luminance(window, "StWindowBg") - Luminance(window, "StText")) > 0.7);
            window.Close();
        });
    }

    [Fact]
    public void SettingsWindow_SwitchesThemeLive_WhenTheUserPicksAnother_OnStaThread()
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings { Theme = "dark" });

        RunOnSta(() =>
        {
            var window = CreateWindow(settings);
            Assert.True(Luminance(window, "StWindowBg") < 0.3);

            window.ViewModel.IsLightTheme = true;

            Assert.True(Luminance(window, "StWindowBg") > 0.8);
            window.Close();
        });
    }

    [Fact]
    public void SettingsWindow_SavesTheChosenTheme_OnStaThread()
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings { Theme = "system" });

        RunOnSta(() =>
        {
            var window = CreateWindow(settings);
            window.ViewModel.IsDarkTheme = true;
            window.ViewModel.SaveAndCloseCommand.Execute(null);
        });

        Assert.Equal("dark", new SettingsService(_settingsPath).LoadSettings().Theme);
    }

    [Fact]
    public void SettingsWindow_SavesTheChosenLanguage_OnStaThread()
    {
        var settings = new SettingsService(_settingsPath);
        settings.SaveSettings(new AppSettings());

        RunOnSta(() =>
        {
            var window = CreateWindow(settings);
            // Mặc định Tiếng Việt
            Assert.True(window.LanguageVietnameseOption.IsChecked);

            window.ViewModel.IsEnglish = true;
            window.ViewModel.SaveAndCloseCommand.Execute(null);
        });

        Assert.Equal("en", new SettingsService(_settingsPath).LoadSettings().Language);
    }

    [Fact]
    public void SettingsWindow_NeverGrowsTallerThanTheScreen_OnStaThread()
    {
        RunOnSta(() =>
        {
            var window = CreateWindow(new SettingsService(_settingsPath));

            Assert.True(window.MaxHeight <= System.Windows.SystemParameters.WorkArea.Height,
                $"MaxHeight {window.MaxHeight} > vùng làm việc {System.Windows.SystemParameters.WorkArea.Height}");
            Assert.NotNull(window.BodyScroller); // nội dung dài thì cuộn, header và nút Lưu luôn hiện
            window.Close();
        });
    }
}
