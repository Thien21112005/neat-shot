using System.Windows;

namespace NeatShot.Core.Models;

/// <summary>
/// Đại diện cho toạ độ và kích thước của vùng chọn chụp màn hình.
/// </summary>
public readonly record struct CaptureRegion(double X, double Y, double Width, double Height)
{
    public static readonly CaptureRegion Empty = default;

    /// <summary>
    /// Vùng chọn hợp lệ khi cả chiều rộng và chiều cao đều lớn hơn 0.
    /// </summary>
    public bool IsValid => Width > 0 && Height > 0;

    /// <summary>
    /// Vùng chọn rỗng khi chiều rộng hoặc chiều cao nhỏ hơn hoặc bằng 0.
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Chuẩn hóa toạ độ vùng chọn khi người dùng kéo chuột ngược chiều
    /// (từ phải qua trái hoặc từ dưới lên trên), đưa về góc trên-trái với width, height dương.
    /// </summary>
    public CaptureRegion Normalize()
    {
        double x = X;
        double y = Y;
        double width = Width;
        double height = Height;

        if (width < 0)
        {
            x += width;
            width = -width;
        }

        if (height < 0)
        {
            y += height;
            height = -height;
        }

        return new CaptureRegion(x, y, width, height);
    }

    /// <summary>
    /// Chuyển đổi sang WPF Rect.
    /// </summary>
    public Rect ToRect() => new(X, Y, Math.Max(0, Width), Math.Max(0, Height));

    /// <summary>
    /// Tạo vùng chọn chuẩn hóa từ 2 điểm toạ độ (điểm bắt đầu và điểm kết thúc kéo chuột).
    /// </summary>
    public static CaptureRegion FromPoints(Point start, Point current)
    {
        return new CaptureRegion(start.X, start.Y, current.X - start.X, current.Y - start.Y).Normalize();
    }
}
