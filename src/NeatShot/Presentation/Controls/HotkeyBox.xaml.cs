using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

/// <summary>
/// Ô gán phím tắt: bấm vào để bắt đầu ghi, nhấn tổ hợp phím để gán, Esc để huỷ.
/// </summary>
public partial class HotkeyBox : UserControl
{
    private static readonly Brush IdleBorder = Frozen(Color.FromArgb(0x29, 0xFF, 0xFF, 0xFF));
    private static readonly Brush RecordingBorder = Frozen(Color.FromRgb(0x00, 0x78, 0xD4));
    private static readonly Brush NormalText = Frozen(Color.FromRgb(0xE8, 0xEA, 0xED));
    private static readonly Brush AccentText = Frozen(Color.FromRgb(0x3B, 0x9B, 0xEB));
    private static readonly Brush WarningText = Frozen(Color.FromRgb(0xFB, 0xBF, 0x24));

    public static readonly DependencyProperty GestureProperty =
        DependencyProperty.Register(
            nameof(Gesture),
            typeof(HotkeyGesture),
            typeof(HotkeyBox),
            new FrameworkPropertyMetadata(default(HotkeyGesture), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) => ((HotkeyBox)d).Refresh()));

    public HotkeyGesture Gesture
    {
        get => (HotkeyGesture)GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }

    public bool IsRecording { get; private set; }

    /// <summary>Bắt đầu ghi phím (để tạm nhả phím tắt toàn cục).</summary>
    public event EventHandler? RecordingStarted;

    /// <summary>Kết thúc ghi phím (gán xong, huỷ hoặc mất focus).</summary>
    public event EventHandler? RecordingEnded;

    public HotkeyBox()
    {
        InitializeComponent();
        Refresh();

        MouseLeftButtonDown += (s, e) =>
        {
            StartRecording();
            e.Handled = true;
        };
        PreviewKeyDown += (s, e) =>
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (HandleKey(key, Keyboard.Modifiers)) e.Handled = true;
        };
        LostKeyboardFocus += (s, e) => StopRecording();
    }

    public void StartRecording()
    {
        if (IsRecording) return;
        IsRecording = true;
        Focus();
        Keyboard.Focus(this);
        ShowPrompt("Nhấn tổ hợp phím…", AccentText);
        RecordingStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Xử lý một lần nhấn phím khi đang ghi. Trả về true nếu phím đã được dùng.
    /// </summary>
    public bool HandleKey(Key key, ModifierKeys modifiers)
    {
        if (!IsRecording) return false;

        if (key == Key.Escape && modifiers == ModifierKeys.None)
        {
            StopRecording();
            return true;
        }

        var candidate = new HotkeyGesture(key, modifiers);
        if (IsModifierKey(key))
        {
            ShowPrompt(modifiers == ModifierKeys.None ? "Nhấn tổ hợp phím…" : $"{ModifierText(modifiers)} + …", AccentText);
            return true;
        }

        if (!candidate.IsValid)
        {
            ShowPrompt($"Cần kèm Ctrl, Alt hoặc Win — ví dụ Ctrl + Shift + {candidate.DisplayText}", WarningText);
            return true;
        }

        Gesture = candidate;
        StopRecording();
        return true;
    }

    private void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        Refresh();
        RecordingEnded?.Invoke(this, EventArgs.Empty);
    }

    private void Refresh()
    {
        if (IsRecording) return;
        DisplayText.Text = Gesture.Key == Key.None ? "Chưa đặt" : Gesture.DisplayText;
        DisplayText.Foreground = NormalText;
        Frame.BorderBrush = IdleBorder;
        EditHint.Visibility = Visibility.Visible;
    }

    private void ShowPrompt(string text, Brush foreground)
    {
        DisplayText.Text = text;
        DisplayText.Foreground = foreground;
        Frame.BorderBrush = RecordingBorder;
        EditHint.Visibility = Visibility.Collapsed;
    }

    private static string ModifierText(ModifierKeys modifiers)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        return string.Join(" + ", parts);
    }

    private static bool IsModifierKey(Key key) => key is
        Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or
        Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;

    private static Brush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
