using NeatShot.Core.Interop;
using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace NeatShot.Presentation.Views;

/// <summary>
/// Cửa sổ đếm ngược của chụp hẹn giờ. Không bao giờ lấy focus (WS_EX_NOACTIVATE) để menu/tooltip
/// người dùng đang mở không bị đóng; Esc được đọc trực tiếp từ bàn phím nên không cần focus.
/// </summary>
public partial class CountdownWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private readonly CaptureCountdown _countdown;
    private readonly int _totalSeconds;
    private readonly DispatcherTimer _secondTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer _escapeTimer = new() { Interval = TimeSpan.FromMilliseconds(50) };

    public CountdownWindow(CaptureCountdown countdown)
    {
        InitializeComponent();

        _countdown = countdown ?? throw new ArgumentNullException(nameof(countdown));
        _totalSeconds = Math.Max(1, countdown.Remaining);
        UpdateDisplay(countdown.Remaining);

        _countdown.Ticked += (s, remaining) => UpdateDisplay(remaining);
        _countdown.Completed += (s, e) => StopTimers();
        _countdown.Cancelled += (s, e) => StopTimers();

        _secondTimer.Tick += (s, e) => _countdown.Tick();
        _escapeTimer.Tick += (s, e) =>
        {
            if ((NativeMethods.GetAsyncKeyState(NativeConstants.VK_ESCAPE) & 0x8000) != 0)
            {
                _countdown.Cancel();
            }
        };

        SourceInitialized += (s, e) =>
        {
            var handle = new WindowInteropHelper(this).Handle;
            var style = NativeMethods.GetWindowLong(handle, GWL_EXSTYLE);
            NativeMethods.SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
        };
        Closed += (s, e) => StopTimers();
    }

    /// <summary>
    /// Hiện ở giữa phía trên màn hình chính và bắt đầu đếm.
    /// </summary>
    public void ShowAndStart()
    {
        Show();
        UpdateLayout();
        Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
        Top = 24;

        _secondTimer.Start();
        _escapeTimer.Start();
    }

    private void UpdateDisplay(int remaining)
    {
        SecondsText.Text = Math.Max(0, remaining).ToString();

        // Vòng tiến trình: phần còn lại của thời gian chờ (chu vi hình tròn đường kính 37px ~ 116 / 3 đơn vị nét)
        var fraction = Math.Clamp((double)remaining / _totalSeconds, 0, 1);
        const double circumference = Math.PI * 37 / 3;
        ProgressRing.StrokeDashArray = new System.Windows.Media.DoubleCollection { circumference * fraction, circumference };
    }

    private void StopTimers()
    {
        _secondTimer.Stop();
        _escapeTimer.Stop();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => _countdown.Cancel();
}
