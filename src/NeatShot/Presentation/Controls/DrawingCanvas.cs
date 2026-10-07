using NeatShot.Core.Models;
using NeatShot.Core.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

/// <summary>
/// Canvas vẽ chú thích vector hiệu năng cao (Pencil, Rectangle, Arrow)
/// lưu trữ các nét vẽ dưới dạng DrawingElement và hỗ trợ hoàn tác UndoStack.
/// </summary>
public class DrawingCanvas : FrameworkElement
{
    private bool _isDrawing;
    private DrawingElement? _currentElement;

    public UndoStack<DrawingElement> UndoStack { get; } = new();

    public static readonly DependencyProperty CurrentToolProperty =
        DependencyProperty.Register(
            nameof(CurrentTool),
            typeof(DrawingToolType),
            typeof(DrawingCanvas),
            new PropertyMetadata(DrawingToolType.None));

    public DrawingToolType CurrentTool
    {
        get => (DrawingToolType)GetValue(CurrentToolProperty);
        set => SetValue(CurrentToolProperty, value);
    }

    public static readonly DependencyProperty CurrentColorProperty =
        DependencyProperty.Register(
            nameof(CurrentColor),
            typeof(Color),
            typeof(DrawingCanvas),
            new PropertyMetadata(Colors.Red));

    public Color CurrentColor
    {
        get => (Color)GetValue(CurrentColorProperty);
        set => SetValue(CurrentColorProperty, value);
    }

    public static readonly DependencyProperty CurrentThicknessProperty =
        DependencyProperty.Register(
            nameof(CurrentThickness),
            typeof(double),
            typeof(DrawingCanvas),
            new PropertyMetadata(3.0));

    public double CurrentThickness
    {
        get => (double)GetValue(CurrentThicknessProperty);
        set => SetValue(CurrentThicknessProperty, value);
    }

    public DrawingCanvas()
    {
        UndoStack.StateChanged += (s, e) => InvalidateVisual();
    }

    public void Undo()
    {
        UndoStack.Pop();
        InvalidateVisual();
    }

    public void Clear()
    {
        UndoStack.Clear();
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (CurrentTool == DrawingToolType.None) return;

        var startPoint = e.GetPosition(this);
        _isDrawing = true;
        CaptureMouse();

        _currentElement = new DrawingElement
        {
            ToolType = CurrentTool,
            Color = CurrentColor,
            Thickness = CurrentThickness,
            StartPoint = startPoint,
            EndPoint = startPoint,
            Points = new List<Point> { startPoint },
            Rect = new Rect(startPoint, startPoint)
        };

        InvalidateVisual();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isDrawing || _currentElement == null) return;

        var currentPoint = e.GetPosition(this);

        switch (_currentElement.ToolType)
        {
            case DrawingToolType.Pencil:
                _currentElement.Points.Add(currentPoint);
                break;

            case DrawingToolType.Rectangle:
                var minX = Math.Min(_currentElement.StartPoint.X, currentPoint.X);
                var minY = Math.Min(_currentElement.StartPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - _currentElement.StartPoint.X);
                var height = Math.Abs(currentPoint.Y - _currentElement.StartPoint.Y);
                _currentElement.Rect = new Rect(minX, minY, width, height);
                _currentElement.EndPoint = currentPoint;
                break;

            case DrawingToolType.Arrow:
                _currentElement.EndPoint = currentPoint;
                break;
        }

        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_isDrawing || _currentElement == null) return;

        _isDrawing = false;
        ReleaseMouseCapture();

        UndoStack.Push(_currentElement);
        _currentElement = null;

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // 1. Vẽ tất cả các nét đã lưu trong UndoStack
        foreach (var element in UndoStack.Items)
        {
            RenderElement(dc, element);
        }

        // 2. Vẽ nét vẽ tạm thời đang kéo chuột
        if (_currentElement != null)
        {
            RenderElement(dc, _currentElement);
        }
    }

    private static void RenderElement(DrawingContext dc, DrawingElement element)
    {
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
                if (element.Points.Count > 1)
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
