using NeatShot.Core.Models;
using System.Globalization;
using Xunit;

namespace NeatShot.Tests.Models;

public class GalleryItemTests
{
    [Theory]
    [InlineData(512L, "512 B")]
    [InlineData(2048L, "2 KB")]
    [InlineData(1024L * 1024, "1.0 MB")]
    [InlineData(2_621_440L, "2.5 MB")]
    [InlineData(5_347_738L, "5.1 MB")]
    public void FormattedSize_FormatsBytesWithOneDecimalForMegabytes(long bytes, string expected)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            var item = new GalleryItem { FileSizeBytes = bytes };

            Assert.Equal(expected, item.FormattedSize);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
