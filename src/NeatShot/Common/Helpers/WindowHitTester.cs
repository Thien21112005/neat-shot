using NeatShot.Core.Models;
using System.Windows;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Tìm cửa sổ / control dưới con trỏ để viền sẵn khi người dùng rê chuột ở màn hình chụp.
/// </summary>
public static class WindowHitTester
{
    /// <summary>
    /// Trả về vùng nên được chọn tại điểm <paramref name="point"/>:
    /// cửa sổ cấp cao nằm trên cùng chứa điểm đó, hoặc control con nhỏ nhất bên trong nó (nếu có).
    /// </summary>
    public static Rect? Find(IReadOnlyList<WindowRegion> regions, Point point)
    {
        ArgumentNullException.ThrowIfNull(regions);

        int? topLevel = null;
        foreach (var region in regions)
        {
            if (region.IsChild || !region.Bounds.Contains(point)) continue;
            if (topLevel == null || region.TopLevelIndex < topLevel) topLevel = region.TopLevelIndex;
        }

        if (topLevel == null) return null;

        Rect? best = null;
        foreach (var region in regions)
        {
            if (region.TopLevelIndex != topLevel || !region.Bounds.Contains(point)) continue;

            var area = region.Bounds.Width * region.Bounds.Height;
            if (best == null || area < best.Value.Width * best.Value.Height)
            {
                best = region.Bounds;
            }
        }

        return best;
    }

    /// <summary>
    /// Đổi hình chữ nhật từ pixel vật lý của màn hình sang DIP trong overlay
    /// (gốc toạ độ overlay = góc trên trái màn hình ảo, cùng tỉ lệ với ảnh chụp đóng băng).
    /// </summary>
    public static Rect ToOverlay(Rect physical, double virtualOriginX, double virtualOriginY, double scaleX, double scaleY)
    {
        if (scaleX <= 0) scaleX = 1.0;
        if (scaleY <= 0) scaleY = 1.0;

        return new Rect(
            (physical.X - virtualOriginX) / scaleX,
            (physical.Y - virtualOriginY) / scaleY,
            physical.Width / scaleX,
            physical.Height / scaleY);
    }
}
