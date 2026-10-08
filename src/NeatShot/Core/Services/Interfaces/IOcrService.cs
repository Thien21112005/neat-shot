using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Dịch vụ nhận diện văn bản (OCR) offline từ hình ảnh.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Kiểm tra xem thiết bị hiện tại có hỗ trợ OCR (đã cài đặt ngôn ngữ OCR) hay không.
    /// </summary>
    bool IsOcrSupported();

    /// <summary>
    /// Trích xuất văn bản từ hình ảnh BitmapSource offline qua Windows OCR.
    /// </summary>
    /// <param name="image">Ảnh cần nhận diện chữ.</param>
    /// <param name="cancellationToken">Token hủy tác vụ.</param>
    /// <returns>Văn bản nhận diện được hoặc chuỗi rỗng nếu không có chữ.</returns>
    Task<string> RecognizeTextAsync(BitmapSource image, CancellationToken cancellationToken = default);
}
