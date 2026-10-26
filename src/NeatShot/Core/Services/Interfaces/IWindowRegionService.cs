using NeatShot.Core.Models;

namespace NeatShot.Core.Services.Interfaces;

/// <summary>
/// Liệt kê các cửa sổ đang hiển thị để màn hình chụp tự viền cửa sổ dưới con trỏ.
/// </summary>
public interface IWindowRegionService
{
    /// <summary>
    /// Danh sách cửa sổ cấp cao (và control con) đang hiển thị, theo pixel vật lý, xếp từ trên xuống dưới (z-order).
    /// </summary>
    IReadOnlyList<WindowRegion> GetVisibleWindows();

    /// <summary>
    /// Như <see cref="GetVisibleWindows"/> nhưng đổi sang DIP trong overlay (gốc = góc trên trái màn hình ảo).
    /// </summary>
    IReadOnlyList<WindowRegion> GetOverlayRegions(double scaleX, double scaleY);
}
