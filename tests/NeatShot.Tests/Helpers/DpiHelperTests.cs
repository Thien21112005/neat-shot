using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class DpiHelperTests
{
    [Theory]
    [InlineData(100, 200, 1.0, 1.0, 100, 200)]       // Scale 100%
    [InlineData(100, 200, 1.25, 1.25, 125, 250)]     // Scale 125%
    [InlineData(100, 200, 1.50, 1.50, 150, 300)]     // Scale 150%
    [InlineData(100, 200, 2.0, 2.0, 200, 400)]       // Scale 200%
    public void TransformToDevice_ShouldScalePointsCorrectly(
        double logicalX, double logicalY, double scaleX, double scaleY, double expectedX, double expectedY)
    {
        var logicalPoint = new Point(logicalX, logicalY);
        var devicePoint = DpiHelper.TransformToDevice(logicalPoint, scaleX, scaleY);

        Assert.Equal(expectedX, devicePoint.X, precision: 2);
        Assert.Equal(expectedY, devicePoint.Y, precision: 2);
    }

    [Theory]
    [InlineData(100, 200, 1.0, 1.0, 100, 200)]
    [InlineData(125, 250, 1.25, 1.25, 100, 200)]
    [InlineData(150, 300, 1.50, 1.50, 100, 200)]
    public void TransformFromDevice_ShouldUnscalePointsCorrectly(
        double deviceX, double deviceY, double scaleX, double scaleY, double expectedX, double expectedY)
    {
        var devicePoint = new Point(deviceX, deviceY);
        var logicalPoint = DpiHelper.TransformFromDevice(devicePoint, scaleX, scaleY);

        Assert.Equal(expectedX, logicalPoint.X, precision: 2);
        Assert.Equal(expectedY, logicalPoint.Y, precision: 2);
    }

    [Fact]
    public void TransformRegionToDevice_ShouldScaleRegionCoordinatesAndSize()
    {
        var logicalRegion = new CaptureRegion(100, 50, 400, 200);
        var deviceRegion = DpiHelper.TransformRegionToDevice(logicalRegion, 1.25, 1.25);

        Assert.Equal(125, deviceRegion.X, precision: 2);
        Assert.Equal(62.5, deviceRegion.Y, precision: 2);
        Assert.Equal(500, deviceRegion.Width, precision: 2);
        Assert.Equal(250, deviceRegion.Height, precision: 2);
    }
}
