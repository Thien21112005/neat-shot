using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Triển khai dịch vụ làm đẹp ảnh chụp màn hình bằng kết xuất DrawingVisual và RenderTargetBitmap.
/// </summary>
public class BeautifyService : IBeautifyService
{
    /// <inheritdoc />
    public RenderTargetBitmap ApplyBeautify(BitmapSource source, BeautifyOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        options ??= new BeautifyOptions();

        if (!options.IsEnabled || (options.Padding <= 0 && options.Preset == BeautifyPreset.None))
        {
            if (source is RenderTargetBitmap rtb) return rtb;
            var copy = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, source.Width, source.Height));
            }
            copy.Render(dv);
            copy.Freeze();
            return copy;
        }

        var dpiX = source.DpiX > 0 ? source.DpiX : 96.0;
        var dpiY = source.DpiY > 0 ? source.DpiY : 96.0;
        var scaleX = dpiX / 96.0;
        var scaleY = dpiY / 96.0;

        var padding = Math.Max(0.0, options.Padding);
        var cornerRadius = Math.Max(0.0, options.CornerRadius);

        var contentWidth = source.Width;
        var contentHeight = source.Height;

        var totalWidth = contentWidth + padding * 2;
        var totalHeight = contentHeight + padding * 2;

        var pixelWidth = (int)Math.Max(1, Math.Round(totalWidth * scaleX));
        var pixelHeight = (int)Math.Max(1, Math.Round(totalHeight * scaleY));

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            // 1. Nền gradient hiện đại
            var bgBrush = options.CreateBackgroundBrush();
            bgBrush.Freeze();
            dc.DrawRectangle(bgBrush, null, new Rect(0, 0, totalWidth, totalHeight));

            var imgRect = new Rect(padding, padding, contentWidth, contentHeight);

            // 2. Đổ bóng mềm đa lớp (Soft Multi-Layer Drop Shadow)
            if (padding > 8 && options.ShadowOpacity > 0)
            {
                DrawSoftShadow(dc, imgRect, cornerRadius, options.ShadowBlurRadius, options.ShadowOpacity);
            }

            // 3. Ảnh đã bo tròn 4 góc qua Clip Geometry
            if (cornerRadius > 0)
            {
                var clip = new RectangleGeometry(imgRect, cornerRadius, cornerRadius);
                clip.Freeze();
                dc.PushClip(clip);
                dc.DrawImage(source, imgRect);
                dc.Pop();

                // 4. Đường viền mờ tinh tế phân tách ảnh với nền gradient
                var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)), 1.0);
                borderPen.Freeze();
                dc.DrawRoundedRectangle(null, borderPen, imgRect, cornerRadius, cornerRadius);
            }
            else
            {
                dc.DrawImage(source, imgRect);
            }
        }

        var result = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
        result.Render(visual);
        result.Freeze();
        return result;
    }

    private static void DrawSoftShadow(DrawingContext dc, Rect rect, double cornerRadius, double blurRadius, double maxOpacity)
    {
        const int layers = 5;
        for (int i = layers; i >= 1; i--)
        {
            double progress = (double)i / layers;
            double spread = blurRadius * progress;
            double layerOpacity = (maxOpacity / layers) * (1.0 - progress * 0.35);

            var shadowBrush = new SolidColorBrush(Color.FromArgb((byte)(layerOpacity * 255), 0, 0, 0));
            shadowBrush.Freeze();

            var shadowRect = new Rect(
                rect.X - spread * 0.3,
                rect.Y - spread * 0.1 + (spread * 0.4),
                rect.Width + spread * 0.6,
                rect.Height + spread * 0.6);

            dc.DrawRoundedRectangle(
                shadowBrush,
                null,
                shadowRect,
                cornerRadius + spread * 0.3,
                cornerRadius + spread * 0.3);
        }
    }
}
