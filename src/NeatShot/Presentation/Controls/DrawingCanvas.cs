using NeatShot.Common.Helpers;
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
            DrawingRenderer.RenderElement(dc, element);
        }

        // 2. Vẽ nét vẽ tạm thời đang kéo chuột
        if (_currentElement != null)
        {
            DrawingRenderer.RenderElement(dc, _currentElement);
        }
    }
}
