using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Services;

public class BeautifyServiceTests
{
    [Fact]
    public void ApplyBeautify_ThrowsArgumentNullException_WhenSourceIsNull()
    {
        var service = new BeautifyService();
        Assert.Throws<ArgumentNullException>(() => { service.ApplyBeautify(null!); });
    }

    [Fact]
    public void ApplyBeautify_IncreasesDimensions_ByPadding_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var service = new BeautifyService();
                var source = new RenderTargetBitmap(100, 80, 96, 96, PixelFormats.Pbgra32);
                var options = new BeautifyOptions
                {
                    Padding = 32.0,
                    CornerRadius = 12.0,
                    Preset = BeautifyPreset.Sunset
                };

                var result = service.ApplyBeautify(source, options);

                Assert.NotNull(result);
                Assert.Equal(164, result.PixelWidth);
                Assert.Equal(144, result.PixelHeight);
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }

    [Fact]
    public void ApplyBeautify_SupportsAllGradientPresets_OnStaThread()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var service = new BeautifyService();
                var source = new RenderTargetBitmap(60, 40, 96, 96, PixelFormats.Pbgra32);

                foreach (var preset in Enum.GetValues<BeautifyPreset>())
                {
                    var options = new BeautifyOptions
                    {
                        Padding = 16.0,
                        Preset = preset
                    };

                    var result = service.ApplyBeautify(source, options);
                    Assert.NotNull(result);
                    Assert.Equal(92, result.PixelWidth);
                    Assert.Equal(72, result.PixelHeight);
                }
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadEx);
    }
}
