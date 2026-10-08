using NeatShot.Core.Models;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Models;

public class ColorHistoryTests
{
    [Fact]
    public void ColorHistory_AddsAndDeduplicatesColors_UpToCapacity()
    {
        var history = new ColorHistory(capacity: 3);

        history.AddColor(Colors.Red);
        history.AddColor(Colors.Green);
        history.AddColor(Colors.Blue);

        Assert.Equal(3, history.Colors.Count);
        Assert.Equal(Colors.Blue, history.Colors[0]);

        // Add Red again: Red should be moved to front (index 0), no duplicate
        history.AddColor(Colors.Red);
        Assert.Equal(3, history.Colors.Count);
        Assert.Equal(Colors.Red, history.Colors[0]);

        // Add Yellow: exceeds capacity of 3 -> oldest color (Green) is evicted
        history.AddColor(Colors.Yellow);
        Assert.Equal(3, history.Colors.Count);
        Assert.Equal(Colors.Yellow, history.Colors[0]);
        Assert.DoesNotContain(Colors.Green, history.Colors);
    }

    [Fact]
    public void ColorHistory_Constructor_DefaultsToCapacity8()
    {
        var history = new ColorHistory();
        Assert.Equal(8, history.Capacity);
        Assert.Empty(history.Colors);
    }

    [Fact]
    public void ColorHistory_Clear_RemovesAllColors()
    {
        var history = new ColorHistory(capacity: 5);
        history.AddColor(Colors.Red);
        history.AddColor(Colors.Blue);

        Assert.Equal(2, history.Colors.Count);

        history.Clear();
        Assert.Empty(history.Colors);
    }
}
