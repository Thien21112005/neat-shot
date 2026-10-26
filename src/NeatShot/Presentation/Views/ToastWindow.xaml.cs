using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NeatShot.Presentation.Views;

/// <summary>
/// Kiểu thông báo: thành công (dấu ✓ xanh) hoặc cảnh báo (dấu ! vàng).
/// </summary>
public enum ToastKind
{
    Success,
    Warning
}

/// <summary>
/// Thông báo nhỏ ở góc phải dưới màn hình (không lấy focus), tự ẩn sau vài giây.
/// </summary>
public partial class ToastWindow : Window
{
    private static readonly TimeSpan VisibleDuration = TimeSpan.FromSeconds(2.4);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(220);

    public ToastWindow(string message, string? detail, ToastKind kind = ToastKind.Success)
    {
        InitializeComponent();

        SuccessIcon.Visibility = kind == ToastKind.Success ? Visibility.Visible : Visibility.Collapsed;
        WarningIcon.Visibility = kind == ToastKind.Warning ? Visibility.Visible : Visibility.Collapsed;

        MessageText.Text = message;
        DetailText.Text = detail ?? string.Empty;
        DetailText.Visibility = string.IsNullOrWhiteSpace(detail) ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Hiển thị toast "Đã sao chép" sau khi chụp, kèm nơi ảnh đã được lưu (nếu có).
    /// </summary>
    public static void ShowCopied(string? savedPath)
    {
        new ToastWindow("Đã sao chép vào Clipboard", DescribeSavedLocation(savedPath)).ShowNearTaskbar();
    }

    /// <summary>
    /// Hiển thị một thông báo nhanh ở góc màn hình.
    /// </summary>
    public static void Show(string message, string? detail = null, ToastKind kind = ToastKind.Success)
    {
        new ToastWindow(message, detail, kind).ShowNearTaskbar();
    }

    /// <summary>
    /// Mô tả ngắn gọn thư mục đã lưu ảnh, ví dụ "Đã lưu vào Pictures\NeatShot".
    /// </summary>
    public static string? DescribeSavedLocation(string? savedPath)
    {
        if (string.IsNullOrWhiteSpace(savedPath)) return null;

        var directory = Path.GetDirectoryName(savedPath);
        if (string.IsNullOrEmpty(directory)) return null;

        var folder = Path.GetFileName(directory);
        var parent = Path.GetFileName(Path.GetDirectoryName(directory) ?? string.Empty);
        var display = string.IsNullOrEmpty(parent)
            ? directory.TrimEnd(Path.DirectorySeparatorChar)
            : Path.Combine(parent, folder);

        return $"Đã lưu vào {display}";
    }

    /// <summary>
    /// Đặt toast ở góc phải dưới vùng làm việc (phía trên taskbar), fade in rồi tự đóng.
    /// </summary>
    public void ShowNearTaskbar()
    {
        Show();
        UpdateLayout();

        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 8;
        Top = workArea.Bottom - ActualHeight - 8;

        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, FadeDuration));

        var timer = new DispatcherTimer { Interval = VisibleDuration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var fadeOut = new DoubleAnimation(1, 0, FadeDuration);
            fadeOut.Completed += (_, _) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        };
        timer.Start();
    }
}
