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
    Ellipse,
    Line,
    Arrow,
    Highlight,
    Text,
    Select,
    Eyedropper,
    Pixelate,
    Blur,
    StepCounter
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
    /// Kích thước chữ (dùng cho công cụ Text).
    /// </summary>
    public double FontSize { get; set; } = 16.0;

    /// <summary>
    /// Phông chữ hiển thị (dùng cho công cụ Text, ví dụ: "Segoe UI", "Arial", "Times New Roman").
    /// </summary>
    public string FontFamily { get; set; } = "Segoe UI";

    /// <summary>
    /// Danh sách các điểm toạ độ dùng cho nét vẽ tự do (Pencil, Highlight).
    /// </summary>
    public List<Point> Points { get; set; } = new();

    /// <summary>
    /// Vùng hình chữ nhật (dùng cho Rectangle, Ellipse, hoặc bounding box của Text).
    /// </summary>
    public Rect Rect { get; set; }

    /// <summary>
    /// Điểm bắt đầu của mũi tên, đường kẻ hoặc văn bản.
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

    /// <summary>
    /// Kích thước ô vuông mosaic (dùng cho công cụ Pixelate).
    /// </summary>
    public int PixelateBlockSize { get; set; } = 10;

    /// <summary>
    /// Bán kính làm mờ (dùng cho công cụ Blur).
    /// </summary>
    public int BlurRadius { get; set; } = 8;

    /// <summary>
    /// Bitmap chứa hiệu ứng hình ảnh đã xử lý (Pixelate, Blur).
    /// </summary>
    public System.Windows.Media.Imaging.BitmapSource? EffectBitmap { get; set; }

    /// <summary>
    /// Số thứ tự bước (dùng cho công cụ StepCounter, ví dụ: 1, 2, 3...).
    /// </summary>
    public int StepNumber { get; set; } = 1;

    /// <summary>
    /// Tính toán hộp bao quanh (bounding box) của phần tử vẽ.
    /// </summary>
    public Rect GetBoundingBox()
    {
        switch (ToolType)
        {
            case DrawingToolType.Rectangle:
            case DrawingToolType.Ellipse:
            case DrawingToolType.Pixelate:
            case DrawingToolType.Blur:
                return Rect;

            case DrawingToolType.StepCounter:
                if (!Rect.IsEmpty && Rect.Width > 0 && Rect.Height > 0)
                {
                    return Rect;
                }
                return new Rect(StartPoint.X - 14, StartPoint.Y - 14, 28, 28);

            case DrawingToolType.Line:
            case DrawingToolType.Arrow:
                var minX = Math.Min(StartPoint.X, EndPoint.X);
                var minY = Math.Min(StartPoint.Y, EndPoint.Y);
                var w = Math.Abs(EndPoint.X - StartPoint.X);
                var h = Math.Abs(EndPoint.Y - StartPoint.Y);
                return new Rect(minX, minY, w, h);

            case DrawingToolType.Pencil:
            case DrawingToolType.Highlight:
                if (Points != null && Points.Count > 0)
                {
                    var pMinX = Points.Min(p => p.X);
                    var pMaxX = Points.Max(p => p.X);
                    var pMinY = Points.Min(p => p.Y);
                    var pMaxY = Points.Max(p => p.Y);
                    return new Rect(pMinX, pMinY, Math.Max(0, pMaxX - pMinX), Math.Max(0, pMaxY - pMinY));
                }
                return Rect.IsEmpty ? new Rect(StartPoint, EndPoint) : Rect;

            case DrawingToolType.Text:
                if (!Rect.IsEmpty && Rect.Width > 0 && Rect.Height > 0)
                {
                    return Rect;
                }
                var textLen = string.IsNullOrEmpty(Text) ? 1 : Text.Length;
                var estWidth = textLen * FontSize * 0.65;
                var estHeight = FontSize * 1.3;
                return new Rect(StartPoint.X, StartPoint.Y, estWidth, estHeight);

            default:
                return Rect.IsEmpty ? new Rect(StartPoint, EndPoint) : Rect;
        }
    }
}
