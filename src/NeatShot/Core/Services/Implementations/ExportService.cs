using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Dịch vụ xuất ảnh chụp hoàn thiện: ghép ảnh crop với các nét vẽ vector,
/// lưu file PNG xuống ổ cứng và sao chép an toàn vào Clipboard.
/// </summary>
public class ExportService : IExportService
{
    private readonly IScreenCaptureService _screenCaptureService;

    public ExportService(IScreenCaptureService screenCaptureService)
    {
        _screenCaptureService = screenCaptureService ?? throw new ArgumentNullException(nameof(screenCaptureService));
    }

    /// <inheritdoc />
    public RenderTargetBitmap RenderFinalImage(
        BitmapSource background,
        CaptureRegion region,
        IEnumerable<DrawingElement> annotations)
    {
        ArgumentNullException.ThrowIfNull(background);

        var norm = region.Normalize();
        var width = (int)Math.Max(1, Math.Round(norm.Width));
        var height = (int)Math.Max(1, Math.Round(norm.Height));

        var croppedBackground = _screenCaptureService.Crop(background, norm);

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            // 1. Vẽ nền ảnh đã crop
            dc.DrawImage(croppedBackground, new Rect(0, 0, width, height));

            // 2. Dịch chuyển toạ độ để các nét vẽ khớp với vùng chọn
            dc.PushTransform(new TranslateTransform(-norm.X, -norm.Y));

            // 3. Vẽ tất cả các nét chú thích
            if (annotations != null)
            {
                foreach (var annotation in annotations)
                {
                    DrawingRenderer.RenderElement(dc, annotation);
                }
            }

            dc.Pop();
        }

        var dpiX = background.DpiX > 0 ? background.DpiX : 96.0;
        var dpiY = background.DpiY > 0 ? background.DpiY : 96.0;

        var rtb = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);
        rtb.Render(visual);
        return rtb;
    }

    /// <inheritdoc />
    public Task CopyToClipboardAsync(BitmapSource image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            CopyImageToClipboardWithRetry(image);
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        var thread = new Thread(() =>
        {
            try
            {
                CopyImageToClipboardWithRetry(image);
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        return tcs.Task;
    }

    /// <inheritdoc />
    public async Task SaveToFileAsync(BitmapSource image, string filePath)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        encoder.Save(stream);
        await stream.FlushAsync();
    }

    private static void CopyImageToClipboardWithRetry(BitmapSource image)
    {
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Clipboard.SetImage(image);
                return;
            }
            catch (COMException)
            {
                if (attempt == maxAttempts) throw;
                Thread.Sleep(30);
            }
        }
    }
}
