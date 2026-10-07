using System.Windows;
using System.Windows.Media;

namespace NeatShot.Core.Models;

/// <summary>
/// Các công cụ vẽ chú thích trên ảnh chụp tạm.
/// </summary>
public enum DrawingToolType
{
    None,
    Pencil,
    Rectangle,
    Arrow,
    Text
}

/// <summary>
/// Đại diện cho một đối tượng vẽ vector trên Drawing Canvas.
/// </summary>
public class DrawingElement
{
    public DrawingToolType ToolType { get; set; } = DrawingToolType.None;
    public Color Color { get; set; } = Colors.Red;
    public double Thickness { get; set; } = 3.0;

    /// <summary>
    /// Danh sách các điểm toạ độ dùng cho nét vẽ tự do (Pencil).
    /// </summary>
    public List<Point> Points { get; set; } = new();

    /// <summary>
    /// Vùng hình chữ nhật (dùng cho Rectangle hoặc bounding box của Arrow/Text).
    /// </summary>
    public Rect Rect { get; set; }

    /// <summary>
    /// Điểm bắt đầu của mũi tên hoặc đường kẻ.
    /// </summary>
    public Point StartPoint { get; set; }

    /// <summary>
    /// Điểm kết thúc của mũi tên hoặc đường kẻ.
    /// </summary>
    public Point EndPoint { get; set; }

    /// <summary>
    /// Nội dung văn bản (dùng cho công cụ Text).
    /// </summary>
    public string? Text { get; set; }
}
