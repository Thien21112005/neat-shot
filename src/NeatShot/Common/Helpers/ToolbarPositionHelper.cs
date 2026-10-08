using System.Windows;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Cung cấp thuật toán tính toán vị trí nổi tối ưu và thích ứng (Adaptive Positioning) cho thanh công cụ Toolbar.
/// Đảm bảo thanh công cụ không bao giờ bị tràn khỏi biên màn hình khi vùng chụp quá lớn hoặc nằm sát các góc.
/// </summary>
public static class ToolbarPositionHelper
{
    private const double DefaultMargin = 8.0;

    /// <summary>
    /// Tính toán toạ độ (X, Y) tối ưu cho thanh công cụ nổi dựa trên vùng chụp, kích thước Toolbar và kích thước màn hình.
    /// </summary>
    /// <param name="regionRect">Hình chữ nhật của vùng chọn đang chụp.</param>
    /// <param name="toolbarSize">Kích thước chiều rộng và chiều cao thực tế của thanh Toolbar.</param>
    /// <param name="screenSize">Kích thước màn hình ảo hiển thị.</param>
    /// <param name="margin">Khoảng cách lề (mặc định 8px).</param>
    /// <returns>Toạ độ Point (X, Y) trên Canvas.</returns>
    public static Point CalculatePosition(Rect regionRect, Size toolbarSize, Size screenSize, double margin = DefaultMargin)
    {
        if (regionRect.IsEmpty || regionRect.Width <= 0 || regionRect.Height <= 0)
        {
            return new Point(8, 8);
        }

        var tbWidth = toolbarSize.Width > 0 ? toolbarSize.Width : 640;
        var tbHeight = toolbarSize.Height > 0 ? toolbarSize.Height : 44;

        var screenW = screenSize.Width > 0 ? screenSize.Width : 1920;
        var screenH = screenSize.Height > 0 ? screenSize.Height : 1080;

        // 1. Tính toán toạ độ X (Horizontal): Ưu tiên căn phải theo vùng chọn
        var targetX = regionRect.Right - tbWidth;

        // Kẹp chặt X trong phạm vi màn hình để không bao giờ bị cắt xén mép trái hoặc mép phải
        var minX = 8.0;
        var maxX = Math.Max(minX, screenW - tbWidth - 8.0);
        var finalX = Math.Clamp(targetX, minX, maxX);

        // 2. Tính toán toạ độ Y (Vertical) theo các thứ tự ưu tiên thông minh:
        double finalY;

        var canFitBelow = (regionRect.Bottom + margin + tbHeight) <= (screenH - 4.0);
        var canFitAbove = (regionRect.Top - margin - tbHeight) >= 4.0;

        if (canFitBelow)
        {
            // Ưu tiên 1: Đặt ngay phía dưới ngoài vùng chọn
            finalY = regionRect.Bottom + margin;
        }
        else if (canFitAbove)
        {
            // Ưu tiên 2: Đặt ngay phía trên ngoài vùng chọn
            finalY = regionRect.Top - margin - tbHeight;
        }
        else
        {
            // Ưu tiên 3: Vùng chọn quá lớn choán gần hết màn hình (cả trên lẫn dưới đều hết chỗ bên ngoài)
            // Đặt BÊN TRONG góc dưới của vùng chọn
            finalY = regionRect.Bottom - tbHeight - margin;
            if (finalY < regionRect.Top + margin)
            {
                // Nếu vùng chọn còn nhỏ hơn cả chiều cao toolbar, đặt tại mép trên trong
                finalY = regionRect.Top + margin;
            }
        }

        // Kẹp an toàn trong giới hạn màn hình
        var minY = 4.0;
        var maxY = Math.Max(minY, screenH - tbHeight - 4.0);
        finalY = Math.Clamp(finalY, minY, maxY);

        return new Point(Math.Round(finalX), Math.Round(finalY));
    }
}
