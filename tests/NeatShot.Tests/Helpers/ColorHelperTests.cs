using NeatShot.Common.Helpers;
using System.Windows.Media;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ColorHelperTests
{
    [Fact]
    public void ToHex_WhenRed_ShouldReturnStandardHex()
    {
        var red = Color.FromArgb(255, 255, 0, 0);
        var hex = ColorHelper.ToHex(red);

        Assert.Equal("#FFFF0000", hex);
    }

    [Theory]
    [InlineData("#FFFF0000", 255, 255, 0, 0)]
    [InlineData("#FF0000", 255, 255, 0, 0)]
    [InlineData("#00FF00", 255, 0, 255, 0)]
    [InlineData("#0000FF", 255, 0, 0, 255)]
    [InlineData("#80FFFFFF", 128, 255, 255, 255)]
    public void FromHex_ValidHex_ShouldParseExpectedColor(string hex, byte a, byte r, byte g, byte b)
    {
        var color = ColorHelper.FromHex(hex);

        Assert.Equal(a, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("#12345")]
    [InlineData("")]
    public void FromHex_InvalidHex_ShouldReturnFallbackColor(string invalidHex)
    {
        var fallback = Colors.Black;
        var color = ColorHelper.FromHex(invalidHex, fallback);

        Assert.Equal(fallback, color);
    }
}
