using NeatShot.Common.Helpers;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class AppThemeTests
{
    [Theory]
    [InlineData("light", false, true)]
    [InlineData("dark", true, false)]
    [InlineData("system", true, true)]
    [InlineData("system", false, false)]
    [InlineData(null, true, true)]
    [InlineData("không-hợp-lệ", false, false)]
    public void IsLight_FollowsTheChoice_OrWindowsWhenSetToSystem(string? choice, bool windowsUsesLight, bool expected)
    {
        Assert.Equal(expected, AppTheme.IsLight(choice, windowsUsesLight));
    }
}
