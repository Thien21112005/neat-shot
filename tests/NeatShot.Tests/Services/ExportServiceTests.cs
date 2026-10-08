using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class ExportServiceTests
{
    private readonly IExportService _service = new ExportService(new ScreenCaptureService());

    [Fact]
    public void RenderFinalImage_ShouldCompositeBackgroundAndAnnotations()
    {
        // Arrange
        var background = new RenderTargetBitmap(500, 400, 96, 96, PixelFormats.Pbgra32);
        var region = new CaptureRegion(50, 50, 200, 150);

        var annotations = new List<DrawingElement>
        {
            new DrawingElement
            {
                ToolType = DrawingToolType.Rectangle,
                Color = Colors.Red,
                Thickness = 2,
                Rect = new Rect(60, 60, 80, 40)
            }
        };

        // Act
        var finalImage = _service.RenderFinalImage(background, region, annotations);

        // Assert
        Assert.NotNull(finalImage);
        Assert.Equal(200, finalImage.PixelWidth);
        Assert.Equal(150, finalImage.PixelHeight);
    }

    [Fact]
    public async Task SaveToFileAsync_ShouldCreatePngFileOnDisk()
    {
        // Arrange
        var image = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
        var tempFile = Path.Combine(Path.GetTempPath(), $"NeatShot_Test_{Guid.NewGuid():N}.png");

        try
        {
            // Act
            await _service.SaveToFileAsync(image, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var fileInfo = new FileInfo(tempFile);
            Assert.True(fileInfo.Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void RenderFinalImage_WithNullBackground_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _service.RenderFinalImage(null!, new CaptureRegion(0, 0, 100, 100), []));
    }

    [Fact]
    public void RenderFinalImage_WithPencilAndArrowAnnotations_Succeeds()
    {
        // Arrange
        var background = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
        var region = new CaptureRegion(10, 10, 100, 100);

        var annotations = new List<DrawingElement>
        {
            new DrawingElement
            {
                ToolType = DrawingToolType.Pencil,
                Color = Colors.Green,
                Thickness = 3,
                Points = new List<Point> { new(15, 15), new(20, 25), new(30, 35) }
            },
            new DrawingElement
            {
                ToolType = DrawingToolType.Arrow,
                Color = Colors.Blue,
                Thickness = 4,
                StartPoint = new Point(20, 20),
                EndPoint = new Point(80, 80)
            }
        };

        // Act
        var result = _service.RenderFinalImage(background, region, annotations);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.PixelWidth);
        Assert.Equal(100, result.PixelHeight);
    }

    [Fact]
    public async Task SaveToFileAsync_WithNullImage_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.SaveToFileAsync(null!, "dummy.png"));
    }

    [Fact]
    public async Task SaveToFileAsync_WithInvalidFilePath_ThrowsArgumentException()
    {
        var image = new RenderTargetBitmap(50, 50, 96, 96, PixelFormats.Pbgra32);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SaveToFileAsync(image, "   "));
    }

    [Fact]
    public void RenderFinalImage_WithHighDpiBackground_ShouldRenderAtPhysicalResolution()
    {
        var background = new RenderTargetBitmap(500, 375, 120, 120, PixelFormats.Pbgra32);
        var region = new CaptureRegion(100, 100, 200, 100);

        var result = _service.RenderFinalImage(background, region, []);

        Assert.NotNull(result);
        Assert.Equal(250, result.PixelWidth);
        Assert.Equal(125, result.PixelHeight);
        Assert.Equal(120, result.DpiX);
        Assert.Equal(120, result.DpiY);
    }
}
