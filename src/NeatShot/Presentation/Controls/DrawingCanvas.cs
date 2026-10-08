using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Core.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

/// <summary>
/// Canvas vẽ chú thích vector hiệu năng cao (Pencil, Rectangle, Arrow)
/// lưu trữ các nét vẽ dưới dạng DrawingElement và hỗ trợ hoàn tác UndoStack.
/// </summary>
public class DrawingCanvas : Canvas
{
    private bool _isDrawing;
    private DrawingElement? _currentElement;

    public UndoStack<DrawingElement> UndoStack { get; } = new();

    public static readonly DependencyProperty CurrentToolProperty =
        DependencyProperty.Register(
            nameof(CurrentTool),
            typeof(DrawingToolType),
            typeof(DrawingCanvas),
            new PropertyMetadata(DrawingToolType.None, OnCurrentToolChangedCallback));

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
        Background = null;
        IsHitTestVisible = false;
        UndoStack.StateChanged += (s, e) => InvalidateVisual();
    }

    private static void OnCurrentToolChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawingCanvas canvas && e.NewValue is DrawingToolType tool)
        {
            canvas.UpdateToolState(tool);
        }
    }

    private void UpdateToolState(DrawingToolType tool)
    {
        if (tool == DrawingToolType.None)
        {
            IsHitTestVisible = false;
            Background = null;
            Cursor = Cursors.Arrow;
        }
        else
        {
            IsHitTestVisible = true;
            Background = Brushes.Transparent;
            Cursor = tool switch
            {
                DrawingToolType.Pencil => Cursors.Pen,
                DrawingToolType.Rectangle => Cursors.Cross,
                DrawingToolType.Arrow => Cursors.Cross,
                _ => Cursors.Arrow
            };
        }
        InvalidateVisual();
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

    public void OffsetElements(double dx, double dy)
    {
        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001) return;

        foreach (var element in UndoStack.Items)
        {
            element.StartPoint = new Point(element.StartPoint.X + dx, element.StartPoint.Y + dy);
            element.EndPoint = new Point(element.EndPoint.X + dx, element.EndPoint.Y + dy);

            if (element.Points != null)
            {
                for (int i = 0; i < element.Points.Count; i++)
                {
                    element.Points[i] = new Point(element.Points[i].X + dx, element.Points[i].Y + dy);
                }
            }

            if (!element.Rect.IsEmpty)
            {
                element.Rect = new Rect(element.Rect.X + dx, element.Rect.Y + dy, element.Rect.Width, element.Rect.Height);
            }
        }

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
        e.Handled = true;
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
        e.Handled = true;
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
        e.Handled = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // 1. Vẽ tất cả các nét đã lưu trong UndoStack
        foreach (var element in UndoStack.Items)
        {
            DrawingRenderer.RenderElement(dc, element);
        }

        // 2. Vẽ nét vẽ tạm thời đang kéo chuột
        if (_currentElement != null)
        {
            DrawingRenderer.RenderElement(dc, _currentElement);
        }
    }
}
