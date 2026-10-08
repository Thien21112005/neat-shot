using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class ExportServiceAdvancedTests
{
    private readonly IExportService _service = new ExportService(new ScreenCaptureService());

    [Fact]
    public void ExportService_RendersAllNewAnnotationTypes_Correctly()
    {
        var background = new RenderTargetBitmap(500, 400, 96, 96, PixelFormats.Pbgra32);
        var region = new CaptureRegion(50, 50, 200, 150);

        var annotations = new List<DrawingElement>
        {
            new() { ToolType = DrawingToolType.Ellipse, Rect = new Rect(60, 60, 40, 30), Color = Colors.Red, Thickness = 2 },
            new() { ToolType = DrawingToolType.Line, StartPoint = new Point(70, 70), EndPoint = new Point(120, 120), Color = Colors.Green, Thickness = 2 },
            new() { ToolType = DrawingToolType.Highlight, Points = new List<Point> { new(80, 80), new(150, 80) }, Color = Colors.Yellow, Thickness = 16 },
            new() { ToolType = DrawingToolType.Text, StartPoint = new Point(90, 90), Text = "Test Note", Color = Colors.White, FontSize = 16, FontFamily = "Arial" }
        };

        var finalImage = _service.RenderFinalImage(background, region, annotations);

        Assert.NotNull(finalImage);
        Assert.Equal(200, finalImage.PixelWidth);
        Assert.Equal(150, finalImage.PixelHeight);
    }

    [Fact]
    public async Task ExportService_ExportsCompositedImageWithAdvancedAnnotations_ToDisk()
    {
        var background = new RenderTargetBitmap(400, 300, 96, 96, PixelFormats.Pbgra32);
        var region = new CaptureRegion(20, 20, 150, 100);

        var annotations = new List<DrawingElement>
        {
            new() { ToolType = DrawingToolType.Ellipse, Rect = new Rect(30, 30, 50, 40), Color = Colors.Cyan, Thickness = 3 },
            new() { ToolType = DrawingToolType.Highlight, Points = new List<Point> { new(40, 50), new(80, 50) }, Color = Colors.Magenta, Thickness = 18 },
            new() { ToolType = DrawingToolType.Text, StartPoint = new Point(50, 70), Text = "Quick Note", Color = Colors.Yellow, FontSize = 14, FontFamily = "Times New Roman" }
        };

        var finalImage = _service.RenderFinalImage(background, region, annotations);
        Assert.NotNull(finalImage);

        var tempFile = Path.Combine(Path.GetTempPath(), $"NeatShot_Advanced_{Guid.NewGuid():N}.png");
        try
        {
            await _service.SaveToFileAsync(finalImage, tempFile);
            Assert.True(File.Exists(tempFile));
            var info = new FileInfo(tempFile);
            Assert.True(info.Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
