using NeatShot.Common.Helpers;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class ClipboardImageReaderTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "NeatShot_ClipTests_" + Guid.NewGuid().ToString("N"));

    public ClipboardImageReaderTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* bỏ qua */ }
    }

    private static void RunOnSta(Action action)
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { threadException = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(threadException);
    }

    private static BitmapSource Bitmap(int width, int height)
    {
        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Freeze();
        return rtb;
    }

    private static byte[] PngBytes(BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    [Fact]
    public void Read_PrefersThePngFormat_UsedByBrowsersAndDesignTools()
    {
        RunOnSta(() =>
        {
            var data = new DataObject();
            data.SetData("PNG", new MemoryStream(PngBytes(Bitmap(120, 80))));

            var image = ClipboardImageReader.Read(data);

            Assert.NotNull(image);
            Assert.Equal(120, image!.PixelWidth);
            Assert.True(image.IsFrozen);
        });
    }

    [Fact]
    public void Read_FallsBackToTheStandardBitmapFormat()
    {
        RunOnSta(() =>
        {
            var data = new DataObject();
            data.SetImage(Bitmap(64, 48));

            var image = ClipboardImageReader.Read(data);

            Assert.NotNull(image);
            Assert.Equal(64, image!.PixelWidth);
        });
    }

    [Fact]
    public void Read_LoadsTheFirstImageFile_CopiedInExplorer()
    {
        var path = Path.Combine(_dir, "copied.png");
        File.WriteAllBytes(path, PngBytes(Bitmap(50, 40)));

        RunOnSta(() =>
        {
            var data = new DataObject();
            data.SetFileDropList(new StringCollection { Path.Combine(_dir, "notes.txt"), path });

            var image = ClipboardImageReader.Read(data);

            Assert.NotNull(image);
            Assert.Equal(50, image!.PixelWidth);
        });
    }

    [Fact]
    public void Read_ReturnsNull_WhenThereIsNoImage()
    {
        RunOnSta(() =>
        {
            var data = new DataObject();
            data.SetText("just some text");

            Assert.Null(ClipboardImageReader.Read(data));
            Assert.Null(ClipboardImageReader.Read(null));
        });
    }
}
