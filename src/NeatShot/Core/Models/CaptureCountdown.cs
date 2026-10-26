namespace NeatShot.Core.Models;

/// <summary>
/// Bộ đếm ngược cho chụp hẹn giờ. Không tự chạy timer: bên ngoài gọi <see cref="Tick"/> mỗi giây
/// (giúp kiểm thử dễ và tách khỏi giao diện).
/// </summary>
public class CaptureCountdown
{
    public const int MaxSeconds = 60;

    public CaptureCountdown(int seconds)
    {
        if (seconds is < 1 or > MaxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), $"Thời gian hẹn giờ phải từ 1 đến {MaxSeconds} giây.");
        }

        Remaining = seconds;
    }

    /// <summary>Số giây còn lại.</summary>
    public int Remaining { get; private set; }

    /// <summary>Đã hết giờ hoặc đã bị huỷ.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Phát ra mỗi lần giảm một giây, kèm số giây còn lại.</summary>
    public event EventHandler<int>? Ticked;

    /// <summary>Hết giờ — đến lúc chụp.</summary>
    public event EventHandler? Completed;

    /// <summary>Người dùng huỷ (Esc).</summary>
    public event EventHandler? Cancelled;

    public void Tick()
    {
        if (IsFinished) return;

        Remaining--;
        Ticked?.Invoke(this, Remaining);

        if (Remaining <= 0)
        {
            IsFinished = true;
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Cancel()
    {
        if (IsFinished) return;

        IsFinished = true;
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
