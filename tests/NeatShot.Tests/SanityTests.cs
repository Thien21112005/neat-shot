namespace NeatShot.Tests;

public class SanityTests
{
    [Fact]
    public void SanityCheck_SolutionAndDependencies_ShouldResolve()
    {
        // Assert that the test project can reference the main NeatShot assembly
        var appType = typeof(NeatShot.App);
        Assert.NotNull(appType);
        Assert.Equal("NeatShot.App", appType.FullName);
    }
}
