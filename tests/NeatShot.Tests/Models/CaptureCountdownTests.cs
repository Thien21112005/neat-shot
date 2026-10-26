using NeatShot.Core.Models;
using Xunit;

namespace NeatShot.Tests.Models;

public class CaptureCountdownTests
{
    [Fact]
    public void Tick_CountsDown_AndCompletesAtZero()
    {
        var countdown = new CaptureCountdown(3);
        var ticks = new List<int>();
        var completed = 0;
        countdown.Ticked += (s, remaining) => ticks.Add(remaining);
        countdown.Completed += (s, e) => completed++;

        countdown.Tick();
        countdown.Tick();
        Assert.Equal(0, completed);
        Assert.Equal(1, countdown.Remaining);

        countdown.Tick();

        Assert.Equal(new[] { 2, 1, 0 }, ticks);
        Assert.Equal(1, completed);
        Assert.True(countdown.IsFinished);
    }

    [Fact]
    public void Cancel_StopsTheCountdown_AndNeverCompletes()
    {
        var countdown = new CaptureCountdown(5);
        var completed = 0;
        var cancelled = 0;
        countdown.Completed += (s, e) => completed++;
        countdown.Cancelled += (s, e) => cancelled++;

        countdown.Tick();
        countdown.Cancel();
        countdown.Tick();
        countdown.Tick();
        countdown.Tick();
        countdown.Tick();

        Assert.Equal(0, completed);
        Assert.Equal(1, cancelled);
        Assert.True(countdown.IsFinished);
    }

    [Fact]
    public void ExtraTicksAfterCompletion_AreIgnored()
    {
        var countdown = new CaptureCountdown(1);
        var completed = 0;
        countdown.Completed += (s, e) => completed++;

        countdown.Tick();
        countdown.Tick();
        countdown.Cancel();

        Assert.Equal(1, completed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(61)]
    public void Constructor_RejectsUnreasonableDelays(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CaptureCountdown(seconds));
    }
}
