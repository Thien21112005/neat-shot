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
}
