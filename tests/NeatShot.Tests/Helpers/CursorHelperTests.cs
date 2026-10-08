using NeatShot.Common.Helpers;
using System.Threading;
using System.Windows.Input;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class CursorHelperTests
{
    [Fact]
    public void CursorHelper_WhiteCrosshair_ReturnsNonNullCursor_OnStaThread()
    {
        Cursor? cursor = null;
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                cursor = CursorHelper.WhiteCrosshair;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.NotNull(cursor);
    }
}
