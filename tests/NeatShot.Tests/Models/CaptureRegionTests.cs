using NeatShot.Core.Models;
using System.Windows;
using Xunit;

namespace NeatShot.Tests.Models;

public class CaptureRegionTests
{
    [Fact]
    public void Normalize_WhenDraggingForward_ShouldPreserveCoordinates()
    {
        // Arrange: Kéo chuột từ trên-trái xuống dưới-phải
        var region = new CaptureRegion(10, 20, 90, 60);

        // Act
        var normalized = region.Normalize();

        // Assert
        Assert.Equal(10, normalized.X);
        Assert.Equal(20, normalized.Y);
        Assert.Equal(90, normalized.Width);
        Assert.Equal(60, normalized.Height);
    }

    [Fact]
    public void Normalize_WhenDraggingRightToLeft_ShouldFlipXAndWidth()
    {
        // Arrange: Kéo từ phải sang trái (Width âm)
        // Bắt đầu tại 100, kéo đến 10 (Width = -90)
        var region = new CaptureRegion(100, 20, -90, 60);

        // Act
        var normalized = region.Normalize();

        // Assert
        Assert.Equal(10, normalized.X);
        Assert.Equal(20, normalized.Y);
        Assert.Equal(90, normalized.Width);
        Assert.Equal(60, normalized.Height);
    }

    [Fact]
    public void Normalize_WhenDraggingBottomToTop_ShouldFlipYAndHeight()
    {
        // Arrange: Kéo từ dưới lên trên (Height âm)
        var region = new CaptureRegion(10, 80, 90, -60);

        // Act
        var normalized = region.Normalize();

        // Assert
        Assert.Equal(10, normalized.X);
        Assert.Equal(20, normalized.Y);
        Assert.Equal(90, normalized.Width);
        Assert.Equal(60, normalized.Height);
    }

    [Fact]
    public void Normalize_WhenDraggingBothAxesInverted_ShouldFlipBoth()
    {
        // Arrange: Kéo từ dưới-phải lên trên-trái
        var region = new CaptureRegion(100, 80, -90, -60);

        // Act
        var normalized = region.Normalize();

        // Assert
        Assert.Equal(10, normalized.X);
        Assert.Equal(20, normalized.Y);
        Assert.Equal(90, normalized.Width);
        Assert.Equal(60, normalized.Height);
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(0, 10, false)]
    [InlineData(10, 0, false)]
    [InlineData(-5, 10, false)]
    public void IsValid_ShouldReturnExpectedResult(double width, double height, bool expected)
    {
        var region = new CaptureRegion(0, 0, width, height);
        Assert.Equal(expected, region.IsValid);
    }

    [Fact]
    public void ToRect_ShouldConvertCorrectlyToWpfRect()
    {
        var region = new CaptureRegion(15, 25, 150, 200);
        var rect = region.ToRect();

        Assert.Equal(15, rect.X);
        Assert.Equal(25, rect.Y);
        Assert.Equal(150, rect.Width);
        Assert.Equal(200, rect.Height);
    }
}
