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

            case DrawingToolType.Pixelate:
            case DrawingToolType.Blur:
                if (element.EffectBitmap != null)
                {
                    dc.DrawImage(element.EffectBitmap, element.Rect);
                }
                else if (!element.Rect.IsEmpty && element.Rect.Width > 0 && element.Rect.Height > 0)
                {
                    var previewPen = new Pen(new SolidColorBrush(Color.FromArgb(180, 0, 120, 212)), 1.5)
                    {
                        DashStyle = DashStyles.Dash
                    };
                    previewPen.Freeze();
                    var previewBrush = new SolidColorBrush(Color.FromArgb(40, 0, 120, 212));
                    previewBrush.Freeze();
                    dc.DrawRectangle(previewBrush, previewPen, element.Rect);
                }
                break;

            case DrawingToolType.Ellipse:
                if (!element.Rect.IsEmpty && element.Rect.Width > 0 && element.Rect.Height > 0)
                {
                    var center = new Point(
                        element.Rect.X + element.Rect.Width / 2.0,
                        element.Rect.Y + element.Rect.Height / 2.0);
                    dc.DrawEllipse(null, pen, center, element.Rect.Width / 2.0, element.Rect.Height / 2.0);
                }
                break;

            case DrawingToolType.Line:
                dc.DrawLine(pen, element.StartPoint, element.EndPoint);
                break;

            case DrawingToolType.Arrow:
                DrawArrow(dc, pen, brush, element.StartPoint, element.EndPoint, element.Thickness);
                break;

            case DrawingToolType.Highlight:
                if (element.Points != null && element.Points.Count > 1)
                {
                    var highlightColor = Color.FromArgb(120, element.Color.R, element.Color.G, element.Color.B);
                    var highlightBrush = new SolidColorBrush(highlightColor);
                    highlightBrush.Freeze();

                    var highlightPen = new Pen(highlightBrush, Math.Max(16.0, element.Thickness * 3.5))
                    {
                        StartLineCap = PenLineCap.Square,
                        EndLineCap = PenLineCap.Square,
                        LineJoin = PenLineJoin.Round
                    };
                    highlightPen.Freeze();

                    var geometry = new StreamGeometry();
                    using (var ctx = geometry.Open())
                    {
                        ctx.BeginFigure(element.Points[0], false, false);
                        ctx.PolyLineTo(element.Points.Skip(1).ToList(), true, true);
                    }
                    geometry.Freeze();
                    dc.DrawGeometry(null, highlightPen, geometry);
                }
                break;

            case DrawingToolType.Text:
                if (!string.IsNullOrWhiteSpace(element.Text))
                {
                    var fontSize = element.FontSize > 0 ? element.FontSize : 16.0;
                    var fontFamilyName = !string.IsNullOrWhiteSpace(element.FontFamily) ? element.FontFamily : "Segoe UI";
                    var formattedText = new FormattedText(
                        element.Text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(fontFamilyName),
                        fontSize,
                        brush,
                        1.0);
                    dc.DrawText(formattedText, element.StartPoint);
                }
                break;

            case DrawingToolType.StepCounter:
                var radius = element.Thickness > 0 ? element.Thickness : 14.0;
                DrawStepBadge(dc, element.StartPoint, element.StepNumber, element.Color, radius);
                break;
        }
    }

    public static void DrawStepBadge(DrawingContext dc, Point center, int number, Color color, double radius = 14.0)
    {
        var bgBrush = new SolidColorBrush(color);
        bgBrush.Freeze();

        var borderPen = new Pen(Brushes.White, 2.0);
        borderPen.Freeze();

        dc.DrawEllipse(bgBrush, borderPen, center, radius, radius);

        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var fontSize = Math.Max(9.0, radius * 0.95);
        var formattedText = new FormattedText(
            number.ToString(),
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            Brushes.White,
            1.0)
        {
            TextAlignment = TextAlignment.Center
        };

        var textOrigin = new Point(center.X, center.Y - formattedText.Height / 2.0);
        dc.DrawText(formattedText, textOrigin);
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
