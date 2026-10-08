using NeatShot.Core.Services.Interfaces;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Security.Cryptography;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Triển khai dịch vụ OCR offline bằng Windows.Media.Ocr.OcrEngine gốc của hệ điều hành Windows.
/// </summary>
public class WindowsOcrService : IOcrService
{
    /// <inheritdoc/>
    public bool IsOcrSupported()
    {
        try
        {
            return OcrEngine.AvailableRecognizerLanguages.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string> RecognizeTextAsync(BitmapSource image, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (image.PixelWidth <= 0 || image.PixelHeight <= 0)
        {
            return string.Empty;
        }

        var engine = GetOcrEngine();
        if (engine == null)
        {
            throw new InvalidOperationException("Không tìm thấy gói ngôn ngữ OCR nào được hỗ trợ trên thiết bị. Vui lòng cài đặt gói ngôn ngữ (Language Pack) trong Windows Settings.");
        }

        BitmapSource effectiveImage = image;
        if (image.PixelWidth > OcrEngine.MaxImageDimension || image.PixelHeight > OcrEngine.MaxImageDimension)
        {
            double scale = Math.Min((double)OcrEngine.MaxImageDimension / image.PixelWidth, (double)OcrEngine.MaxImageDimension / image.PixelHeight);
            effectiveImage = new TransformedBitmap(image, new ScaleTransform(scale, scale));
        }

        var softwareBitmap = ConvertToSoftwareBitmap(effectiveImage);
        var ocrResult = await engine.RecognizeAsync(softwareBitmap).AsTask(cancellationToken);

        return ocrResult?.Text ?? string.Empty;
    }

    private static OcrEngine? GetOcrEngine()
    {
        try
        {
            // 1. Ưu tiên ngôn ngữ trong thiết lập hồ sơ người dùng Windows
            var engine = OcrEngine.TryCreateFromUserProfileLanguages();
            if (engine != null)
            {
                return engine;
            }

            // 2. Dự phòng: Thử tiếng Anh (en-US)
            var enLanguage = new Windows.Globalization.Language("en-US");
            if (OcrEngine.IsLanguageSupported(enLanguage))
            {
                engine = OcrEngine.TryCreateFromLanguage(enLanguage);
                if (engine != null)
                {
                    return engine;
                }
            }

            // 3. Dự phòng: Chọn ngôn ngữ khả dụng đầu tiên trong hệ thống
            var availableLanguages = OcrEngine.AvailableRecognizerLanguages;
            if (availableLanguages.Count > 0)
            {
                return OcrEngine.TryCreateFromLanguage(availableLanguages[0]);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static SoftwareBitmap ConvertToSoftwareBitmap(BitmapSource image)
    {
        FormatConvertedBitmap convertedBitmap;
        if (image.Format != PixelFormats.Bgra32)
        {
            convertedBitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        }
        else if (image is FormatConvertedBitmap fcb)
        {
            convertedBitmap = fcb;
        }
        else
        {
            convertedBitmap = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        }

        int width = convertedBitmap.PixelWidth;
        int height = convertedBitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelBuffer = new byte[height * stride];
        convertedBitmap.CopyPixels(pixelBuffer, stride, 0);

        var ibuffer = CryptographicBuffer.CreateFromByteArray(pixelBuffer);
        var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
        softwareBitmap.CopyFromBuffer(ibuffer);
        return softwareBitmap;
    }
}
