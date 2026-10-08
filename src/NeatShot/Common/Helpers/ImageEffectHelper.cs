using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Các thuật toán xử lý hiệu ứng làm mờ (Blur) và khảm ô vuông (Pixelate)
/// trực tiếp trên mảng byte pixel hiệu năng cao O(N).
/// </summary>
public static class ImageEffectHelper
{
    /// <summary>
    /// Áp dụng hiệu ứng khảm điểm ảnh (Pixelate / Mosaic) lên vùng chỉ định của ảnh.
    /// </summary>
    /// <param name="source">Ảnh gốc.</param>
    /// <param name="region">Vùng áp dụng hiệu ứng (tính theo DIPs).</param>
    /// <param name="blockSize">Kích thước một ô vuông mosaic (mặc định 10 px).</param>
    /// <returns>Ảnh BitmapSource chứa phần ảnh đã khảm ô vuông.</returns>
    public static BitmapSource ApplyPixelate(BitmapSource source, Rect region, int blockSize = 10)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (cropX, cropY, cropW, cropH) = GetPixelBounds(source, region);
        if (cropW <= 0 || cropH <= 0)
        {
            return new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
        }

        blockSize = Math.Max(2, blockSize);

        var dpiX = source.DpiX > 0 ? source.DpiX : 96.0;
        var dpiY = source.DpiY > 0 ? source.DpiY : 96.0;

        // Trích xuất buffer Bgra32
        var pixels = ExtractPixels(source, cropX, cropY, cropW, cropH, out var stride);

        // Thuật toán khảm điểm ảnh O(N)
        for (int y = 0; y < cropH; y += blockSize)
        {
            int blockH = Math.Min(blockSize, cropH - y);

            for (int x = 0; x < cropW; x += blockSize)
            {
                int blockW = Math.Min(blockSize, cropW - x);
                int totalPixels = blockW * blockH;

                long sumB = 0, sumG = 0, sumR = 0, sumA = 0;

                // 1. Tính màu trung bình trong block
                for (int by = 0; by < blockH; by++)
                {
                    int rowOffset = (y + by) * stride;
                    for (int bx = 0; bx < blockW; bx++)
                    {
                        int idx = rowOffset + (x + bx) * 4;
                        sumB += pixels[idx];
                        sumG += pixels[idx + 1];
                        sumR += pixels[idx + 2];
                        sumA += pixels[idx + 3];
                    }
                }

                byte avgB = (byte)(sumB / totalPixels);
                byte avgG = (byte)(sumG / totalPixels);
                byte avgR = (byte)(sumR / totalPixels);
                byte avgA = (byte)(sumA / totalPixels);

                // 2. Ghi đè màu trung bình vào toàn bộ block
                for (int by = 0; by < blockH; by++)
                {
                    int rowOffset = (y + by) * stride;
                    for (int bx = 0; bx < blockW; bx++)
                    {
                        int idx = rowOffset + (x + bx) * 4;
                        pixels[idx] = avgB;
                        pixels[idx + 1] = avgG;
                        pixels[idx + 2] = avgR;
                        pixels[idx + 3] = avgA;
                    }
                }
            }
        }

        var result = BitmapSource.Create(cropW, cropH, dpiX, dpiY, PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    /// <summary>
    /// Áp dụng hiệu ứng làm mờ nhanh (Fast Box Blur) lên vùng chỉ định của ảnh.
    /// Sử dụng kỹ thuật 2-Pass tích lũy trượt (Sliding Window) chạy trong O(N).
    /// </summary>
    /// <param name="source">Ảnh gốc.</param>
    /// <param name="region">Vùng áp dụng hiệu ứng (tính theo DIPs).</param>
    /// <param name="radius">Bán kính làm mờ (mặc định 8 px).</param>
    /// <returns>Ảnh BitmapSource chứa phần ảnh đã làm mờ.</returns>
    public static BitmapSource ApplyBoxBlur(BitmapSource source, Rect region, int radius = 8)
    {
        ArgumentNullException.ThrowIfNull(source);

        var (cropX, cropY, cropW, cropH) = GetPixelBounds(source, region);
        if (cropW <= 0 || cropH <= 0)
        {
            return new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
        }

        radius = Math.Max(1, radius);

        var dpiX = source.DpiX > 0 ? source.DpiX : 96.0;
        var dpiY = source.DpiY > 0 ? source.DpiY : 96.0;

        var pixels = ExtractPixels(source, cropX, cropY, cropW, cropH, out var stride);
        var buffer = new byte[pixels.Length];

        // Chạy 2 lượt Box Blur để xấp xỉ Gaussian Blur mềm mại
        for (int pass = 0; pass < 2; pass++)
        {
            BoxBlurHorizontal(pixels, buffer, cropW, cropH, stride, radius);
            BoxBlurVertical(buffer, pixels, cropW, cropH, stride, radius);
        }

        var result = BitmapSource.Create(cropW, cropH, dpiX, dpiY, PixelFormats.Bgra32, null, pixels, stride);
        result.Freeze();
        return result;
    }

    private static void BoxBlurHorizontal(byte[] src, byte[] dst, int w, int h, int stride, int r)
    {
        for (int y = 0; y < h; y++)
        {
            int rowOffset = y * stride;

            long sumB = 0, sumG = 0, sumR = 0, sumA = 0;

            // Khởi tạo cửa sổ trượt ban đầu
            for (int x = -r; x <= r; x++)
            {
                int clampedX = Math.Clamp(x, 0, w - 1);
                int idx = rowOffset + clampedX * 4;
                sumB += src[idx];
                sumG += src[idx + 1];
                sumR += src[idx + 2];
                sumA += src[idx + 3];
            }

            int windowSize = 2 * r + 1;

            for (int x = 0; x < w; x++)
            {
                int outIdx = rowOffset + x * 4;
                dst[outIdx] = (byte)(sumB / windowSize);
                dst[outIdx + 1] = (byte)(sumG / windowSize);
                dst[outIdx + 2] = (byte)(sumR / windowSize);
                dst[outIdx + 3] = (byte)(sumA / windowSize);

                // Cửa sổ trượt: bỏ điểm ngoài biên trái, nạp điểm mới ở biên phải
                int leftX = Math.Clamp(x - r, 0, w - 1);
                int rightX = Math.Clamp(x + r + 1, 0, w - 1);

                int leftIdx = rowOffset + leftX * 4;
                int rightIdx = rowOffset + rightX * 4;

                sumB += src[rightIdx] - src[leftIdx];
                sumG += src[rightIdx + 1] - src[leftIdx + 1];
                sumR += src[rightIdx + 2] - src[leftIdx + 2];
                sumA += src[rightIdx + 3] - src[leftIdx + 3];
            }
        }
    }

    private static void BoxBlurVertical(byte[] src, byte[] dst, int w, int h, int stride, int r)
    {
        int windowSize = 2 * r + 1;

        for (int x = 0; x < w; x++)
        {
            int colOffset = x * 4;

            long sumB = 0, sumG = 0, sumR = 0, sumA = 0;

            for (int y = -r; y <= r; y++)
            {
                int clampedY = Math.Clamp(y, 0, h - 1);
                int idx = clampedY * stride + colOffset;
                sumB += src[idx];
                sumG += src[idx + 1];
                sumR += src[idx + 2];
                sumA += src[idx + 3];
            }

            for (int y = 0; y < h; y++)
            {
                int outIdx = y * stride + colOffset;
                dst[outIdx] = (byte)(sumB / windowSize);
                dst[outIdx + 1] = (byte)(sumG / windowSize);
                dst[outIdx + 2] = (byte)(sumR / windowSize);
                dst[outIdx + 3] = (byte)(sumA / windowSize);

                int topY = Math.Clamp(y - r, 0, h - 1);
                int bottomY = Math.Clamp(y + r + 1, 0, h - 1);

                int topIdx = topY * stride + colOffset;
                int bottomIdx = bottomY * stride + colOffset;

                sumB += src[bottomIdx] - src[topIdx];
                sumG += src[bottomIdx + 1] - src[topIdx + 1];
                sumR += src[bottomIdx + 2] - src[topIdx + 2];
                sumA += src[bottomIdx + 3] - src[topIdx + 3];
            }
        }
    }

    private static (int x, int y, int w, int h) GetPixelBounds(BitmapSource source, Rect region)
    {
        var norm = new Rect(
            Math.Min(region.Left, region.Right),
            Math.Min(region.Top, region.Bottom),
            Math.Abs(region.Width),
            Math.Abs(region.Height));

        var dpiX = source.DpiX > 0 ? source.DpiX : 96.0;
        var dpiY = source.DpiY > 0 ? source.DpiY : 96.0;
        var scaleX = dpiX / 96.0;
        var scaleY = dpiY / 96.0;

        int px = (int)Math.Round(norm.X * scaleX);
        int py = (int)Math.Round(norm.Y * scaleY);
        int pw = (int)Math.Round(norm.Width * scaleX);
        int ph = (int)Math.Round(norm.Height * scaleY);

        px = Math.Clamp(px, 0, source.PixelWidth);
        py = Math.Clamp(py, 0, source.PixelHeight);
        pw = Math.Clamp(pw, 0, source.PixelWidth - px);
        ph = Math.Clamp(ph, 0, source.PixelHeight - py);

        return (px, py, pw, ph);
    }

    private static byte[] ExtractPixels(BitmapSource source, int x, int y, int w, int h, out int stride)
    {
        stride = w * 4;
        var pixels = new byte[h * stride];

        // Đảm bảo định dạng Bgra32
        FormatConvertedBitmap converted;
        if (source.Format != PixelFormats.Bgra32)
        {
            converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        }
        else if (source is FormatConvertedBitmap fcb)
        {
            converted = fcb;
        }
        else
        {
            converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        }

        var sourceRect = new Int32Rect(x, y, w, h);
        converted.CopyPixels(sourceRect, pixels, stride, 0);
        return pixels;
    }
}
