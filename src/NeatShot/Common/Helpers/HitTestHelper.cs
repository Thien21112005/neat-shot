using NeatShot.Core.Models;
using System.Windows;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Cung cấp các giải thuật kiểm tra va chạm (hit testing) toạ độ điểm với các hình vẽ chú thích vector.
/// Hỗ trợ lựa chọn và kéo thả đối tượng vẽ trên DrawingCanvas.
/// </summary>
public static class HitTestHelper
{
    /// <summary>
    /// Kiểm tra xem toạ độ testPoint có nằm trên hoặc ở gần hình vẽ element với khoảng cách dung sai (tolerance) hay không.
    /// </summary>
    /// <param name="element">Phần tử vẽ cần kiểm tra va chạm.</param>
    /// <param name="testPoint">Toạ độ điểm cần kiểm tra.</param>
    /// <param name="tolerance">Độ lệch cho phép theo pixel (mặc định 6.0px).</param>
    /// <returns>True nếu điểm chạm vào hình vẽ, ngược lại False.</returns>
    public static bool HitTest(DrawingElement element, Point testPoint, double tolerance = 6.0)
    {
        ArgumentNullException.ThrowIfNull(element);

        var halfThick = Math.Max(1.0, element.Thickness / 2.0);
        var effTol = tolerance + halfThick;

        switch (element.ToolType)
        {
            case DrawingToolType.Rectangle:
            case DrawingToolType.Pixelate:
            case DrawingToolType.Blur:
                return HitTestRectangle(element.Rect, testPoint, effTol);

            case DrawingToolType.Ellipse:
                return HitTestEllipse(element.Rect, testPoint, effTol);

            case DrawingToolType.Line:
                return DistanceToSegment(testPoint, element.StartPoint, element.EndPoint) <= effTol;

            case DrawingToolType.Arrow:
                return HitTestArrow(element, testPoint, effTol);

            case DrawingToolType.Pencil:
            case DrawingToolType.Highlight:
                return HitTestPolyline(element.Points, testPoint, effTol);

            case DrawingToolType.StepCounter:
                var badgeRadius = 14.0 + tolerance;
                return (testPoint - element.StartPoint).Length <= badgeRadius;

            case DrawingToolType.Text:
                var textBounds = element.GetBoundingBox();
                var expandedText = new Rect(
                    textBounds.X - tolerance,
                    textBounds.Y - tolerance,
                    textBounds.Width + 2 * tolerance,
                    textBounds.Height + 2 * tolerance);
                return expandedText.Contains(testPoint);

            default:
                var bounds = element.GetBoundingBox();
                var expanded = new Rect(
                    bounds.X - effTol,
                    bounds.Y - effTol,
                    bounds.Width + 2 * effTol,
                    bounds.Height + 2 * effTol);
                return expanded.Contains(testPoint);
        }
    }

    private static bool HitTestRectangle(Rect rect, Point p, double tolerance)
    {
        if (rect.IsEmpty) return false;

        var outer = new Rect(rect.X - tolerance, rect.Y - tolerance, rect.Width + 2 * tolerance, rect.Height + 2 * tolerance);
        return outer.Contains(p);
    }

    private static bool HitTestEllipse(Rect rect, Point p, double tolerance)
    {
        if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0) return false;

        var cx = rect.X + rect.Width / 2.0;
        var cy = rect.Y + rect.Height / 2.0;
        var rx = rect.Width / 2.0 + tolerance;
        var ry = rect.Height / 2.0 + tolerance;

        var dx = (p.X - cx) / rx;
        var dy = (p.Y - cy) / ry;

        return (dx * dx + dy * dy) <= 1.0;
    }

    private static bool HitTestArrow(DrawingElement element, Point p, double tolerance)
    {
        // 1. Kiểm tra va chạm thân mũi tên
        if (DistanceToSegment(p, element.StartPoint, element.EndPoint) <= tolerance)
        {
            return true;
        }

        // 2. Kiểm tra va chạm đầu mũi tên
        var headRadius = Math.Max(12.0, element.Thickness * 3.5) + tolerance;
        var distToTip = (p - element.EndPoint).Length;
        return distToTip <= headRadius;
    }

    private static bool HitTestPolyline(IReadOnlyList<Point>? points, Point p, double tolerance)
    {
        if (points == null || points.Count == 0) return false;

        if (points.Count == 1)
        {
            return (p - points[0]).Length <= tolerance;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            if (DistanceToSegment(p, points[i], points[i + 1]) <= tolerance)
            {
                return true;
            }
        }

        return false;
    }

    private static double DistanceToSegment(Point p, Point p1, Point p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        var lengthSquared = dx * dx + dy * dy;

        if (lengthSquared < 0.0001)
        {
            return (p - p1).Length;
        }

        // Tỷ lệ chiếu điểm p lên đoạn thẳng [p1, p2]
        var t = ((p.X - p1.X) * dx + (p.Y - p1.Y) * dy) / lengthSquared;
        t = Math.Clamp(t, 0.0, 1.0);

        var projection = new Point(p1.X + t * dx, p1.Y + t * dy);
        return (p - projection).Length;
    }
}
