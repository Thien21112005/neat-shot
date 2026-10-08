using NeatShot.Presentation.ViewModels;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.ViewModels;

public class PinViewModelTests
{
    [Fact]
    public void PinViewModel_InitialValues_AreCorrect()
    {
        var vm = new PinViewModel();

        Assert.Equal(1.0, vm.Scale);
        Assert.Equal(1.0, vm.Opacity);
        Assert.Null(vm.PinnedImage);
        Assert.Equal(0, vm.OriginalWidth);
        Assert.Equal(0, vm.OriginalHeight);
    }

    [Fact]
    public void PinViewModel_ZoomInAndZoomOut_ClampsCorrectly()
    {
        var vm = new PinViewModel();

        // Zoom In increases by 0.1
        vm.ZoomIn();
        Assert.True(Math.Abs(vm.Scale - 1.1) < 0.001);

        // Zoom In cannot exceed 5.0
        for (int i = 0; i < 60; i++)
        {
            vm.ZoomIn();
        }
        Assert.Equal(5.0, vm.Scale);

        // Reset
        vm.ResetZoom();
        Assert.Equal(1.0, vm.Scale);

        // Zoom Out cannot go below 0.2
        for (int i = 0; i < 20; i++)
        {
            vm.ZoomOut();
        }
        Assert.Equal(0.2, vm.Scale);
    }

    [Fact]
    public void PinViewModel_Opacity_ClampsBetweenMinimumAndMaximum()
    {
        var vm = new PinViewModel();

        // Decrease opacity by 0.1
        vm.DecreaseOpacity();
        Assert.True(Math.Abs(vm.Opacity - 0.9) < 0.001);

        // Cannot decrease below 0.2
        for (int i = 0; i < 15; i++)
        {
            vm.DecreaseOpacity();
        }
        Assert.Equal(0.2, vm.Opacity);

        // Increase opacity cannot exceed 1.0
        for (int i = 0; i < 15; i++)
        {
            vm.IncreaseOpacity();
        }
        Assert.Equal(1.0, vm.Opacity);
    }

    [Fact]
    public void PinViewModel_CloseCommand_RaisesRequestClose()
    {
        var vm = new PinViewModel();
        var wasClosed = false;
        vm.RequestClose += (s, e) => wasClosed = true;

        vm.CloseCommand.Execute(null);

        Assert.True(wasClosed);
    }

    [Fact]
    public void PinViewModel_SetImage_CalculatesScaledDimensionsCorrectly()
    {
        var vm = new PinViewModel();
        var dummyBitmap = new RenderTargetBitmap(200, 100, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

        vm.SetImage(dummyBitmap);

        Assert.Equal(200, vm.OriginalWidth);
        Assert.Equal(100, vm.OriginalHeight);
        Assert.Equal(200, vm.ScaledWidth);
        Assert.Equal(100, vm.ScaledHeight);

        vm.ZoomIn(); // Scale 1.1
        Assert.True(Math.Abs(vm.ScaledWidth - 220) < 0.01);
        Assert.True(Math.Abs(vm.ScaledHeight - 110) < 0.01);
    }
}
