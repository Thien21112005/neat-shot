using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Media;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Lớp hỗ trợ vẽ các phần tử chú thích vector (DrawingElement) lên DrawingContext.
/// Được dùng chung giữa DrawingCanvas (hiển thị tương tác) và ExportService (xuất ảnh cuối cùng).
/// </summary>
public static class DrawingRenderer
{
    public static void RenderElement(DrawingContext dc, DrawingElement element)
    {
        ArgumentNullException.ThrowIfNull(dc);
        ArgumentNullException.ThrowIfNull(element);

        var brush = new SolidColorBrush(element.Color);
        brush.Freeze();

        var pen = new Pen(brush, element.Thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();

        switch (element.ToolType)
        {
            case DrawingToolType.Pencil:
                if (element.Points != null && element.Points.Count > 1)
                {
                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(element.Points[0], false, false);
                        ctx.PolyLineTo(element.Points.Skip(1).ToList(), true, true);
                    }
                    geometry.Freeze();
                    dc.DrawGeometry(null, pen, geometry);
                }
                break;

            case DrawingToolType.Rectangle:
                dc.DrawRectangle(null, pen, element.Rect);
                break;

            case DrawingToolType.Arrow:
                DrawArrow(dc, pen, brush, element.StartPoint, element.EndPoint, element.Thickness);
                break;
        }
    }

    private static void DrawArrow(DrawingContext dc, Pen pen, Brush brush, Point p1, Point p2, double thickness)
    {
        // Vẽ thân mũi tên
        dc.DrawLine(pen, p1, p2);

        // Tính góc đầu mũi tên
        var theta = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        var arrowLength = Math.Max(12, thickness * 3.5);
        var arrowAngle = Math.PI / 6; // 30 độ

        var wing1 = new Point(
            p2.X - arrowLength * Math.Cos(theta - arrowAngle),
            p2.Y - arrowLength * Math.Sin(theta - arrowAngle));

        var wing2 = new Point(
            p2.X - arrowLength * Math.Cos(theta + arrowAngle),
            p2.Y - arrowLength * Math.Sin(theta + arrowAngle));

        // Vẽ tam giác đầu mũi tên
        var arrowHead = new StreamGeometry();
        using (var ctx = arrowHead.Open())
        {
            ctx.BeginFigure(p2, true, true);
            ctx.LineTo(wing1, true, false);
            ctx.LineTo(wing2, true, false);
        }
        arrowHead.Freeze();

        dc.DrawGeometry(brush, null, arrowHead);
    }
}
