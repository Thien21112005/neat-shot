using NeatShot.Core.Models;
using NeatShot.Presentation.Controls;
using System.Threading;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Controls;

public class HotkeyBoxTests
{
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
    public void RecordingAGesture_UpdatesTheValue_AndStopsRecording()
    {
        RunOnSta(() =>
        {
            var box = new HotkeyBox { Gesture = new HotkeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift) };
            var started = 0;
            var ended = 0;
            box.RecordingStarted += (s, e) => started++;
            box.RecordingEnded += (s, e) => ended++;

            box.StartRecording();
            Assert.True(box.IsRecording);
            Assert.Equal("Nhấn tổ hợp phím…", box.DisplayText.Text);

            Assert.True(box.HandleKey(Key.S, ModifierKeys.Control | ModifierKeys.Alt));

            Assert.Equal(new HotkeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt), box.Gesture);
            Assert.False(box.IsRecording);
            Assert.Equal("Ctrl + Alt + S", box.DisplayText.Text);
            Assert.Equal(1, started);
            Assert.Equal(1, ended);
        });
    }

    [Fact]
    public void Escape_CancelsRecording_WithoutChangingTheValue()
    {
        RunOnSta(() =>
        {
            var original = new HotkeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
            var box = new HotkeyBox { Gesture = original };

            box.StartRecording();
            box.HandleKey(Key.Escape, ModifierKeys.None);

            Assert.False(box.IsRecording);
            Assert.Equal(original, box.Gesture);
        });
    }

    [Fact]
    public void ModifierOnlyOrUnsafeKeys_KeepRecording_AndShowAHint()
    {
        RunOnSta(() =>
        {
            var original = new HotkeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
            var box = new HotkeyBox { Gesture = original };
            box.StartRecording();

            box.HandleKey(Key.LeftCtrl, ModifierKeys.Control);
            Assert.True(box.IsRecording);
            Assert.Equal("Ctrl + …", box.DisplayText.Text);

            box.HandleKey(Key.B, ModifierKeys.None);
            Assert.True(box.IsRecording);
            Assert.Equal(original, box.Gesture);
            Assert.Contains("Ctrl", box.DisplayText.Text);
        });
    }

    [Fact]
    public void KeysAreIgnored_WhenNotRecording()
    {
        RunOnSta(() =>
        {
            var original = new HotkeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Shift);
            var box = new HotkeyBox { Gesture = original };

            Assert.False(box.HandleKey(Key.S, ModifierKeys.Control));
            Assert.Equal(original, box.Gesture);
        });
    }
}
