using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.ViewModels;

public class OverlayViewModelTests
{
    [Fact]
    public void InitialState_ShouldHaveEmptySelection()
    {
        var vm = new OverlayViewModel();

        Assert.Null(vm.BackgroundImage);
        Assert.True(vm.SelectedRegion.IsEmpty);
        Assert.False(vm.HasSelection);
    }

    [Fact]
    public void SettingValidSelectedRegion_ShouldUpdateHasSelection()
    {
        var vm = new OverlayViewModel();
        var region = new CaptureRegion(10, 10, 100, 100);

        vm.SelectedRegion = region;

        Assert.Equal(region, vm.SelectedRegion);
        Assert.True(vm.HasSelection);
    }

    [Fact]
    public void Cancel_ShouldResetSelectionAndTriggerRequestClose()
    {
        var vm = new OverlayViewModel();
        vm.SelectedRegion = new CaptureRegion(10, 10, 100, 100);

        var closeRequested = false;
        vm.RequestClose += (s, e) => closeRequested = true;

        vm.CancelCommand.Execute(null);

        Assert.True(closeRequested);
        Assert.True(vm.SelectedRegion.IsEmpty);
        Assert.False(vm.HasSelection);
    }
}
