using NeatShot.Core.Models;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Dịch vụ làm đẹp ảnh chụp màn hình (Beautifier).
/// </summary>
public interface IBeautifyService
{
    /// <summary>
    /// Áp dụng khoảng đệm nghệ thuật, bo tròn góc, đổ bóng mềm và nền gradient.
    /// </summary>
    /// <param name="source">Ảnh gốc cần làm đẹp.</param>
    /// <param name="options">Các tuỳ chọn làm đẹp (nếu null sẽ dùng tuỳ chọn mặc định).</param>
    /// <returns>Ảnh RenderTargetBitmap đã được làm đẹp hoàn chỉnh.</returns>
    RenderTargetBitmap ApplyBeautify(BitmapSource source, BeautifyOptions? options = null);
}
