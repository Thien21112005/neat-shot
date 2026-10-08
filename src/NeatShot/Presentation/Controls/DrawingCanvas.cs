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

    public static readonly DependencyProperty CurrentFontSizeProperty =
        DependencyProperty.Register(
            nameof(CurrentFontSize),
            typeof(double),
            typeof(DrawingCanvas),
            new PropertyMetadata(16.0));

    public double CurrentFontSize
    {
        get => (double)GetValue(CurrentFontSizeProperty);
        set => SetValue(CurrentFontSizeProperty, value);
    }

    public static readonly DependencyProperty CurrentFontFamilyProperty =
        DependencyProperty.Register(
            nameof(CurrentFontFamily),
            typeof(string),
            typeof(DrawingCanvas),
            new PropertyMetadata("Segoe UI"));

    public string CurrentFontFamily
    {
        get => (string)GetValue(CurrentFontFamilyProperty);
        set => SetValue(CurrentFontFamilyProperty, value);
    }

    private TextBox? _inlineEditor;
    private Point _inlineEditorPosition;

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
                DrawingToolType.Highlight => Cursors.Pen,
                DrawingToolType.Rectangle => Cursors.Cross,
                DrawingToolType.Ellipse => Cursors.Cross,
                DrawingToolType.Line => Cursors.Cross,
                DrawingToolType.Arrow => Cursors.Cross,
                DrawingToolType.Text => Cursors.IBeam,
                DrawingToolType.Select => Cursors.Hand,
                DrawingToolType.Eyedropper => Cursors.Cross,
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

    public void Redo()
    {
        UndoStack.Redo();
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

    public void StartDrawing(Point startPoint)
    {
        if (CurrentTool == DrawingToolType.None) return;

        _isDrawing = true;
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

    public void MoveDrawing(Point currentPoint)
    {
        if (!_isDrawing || _currentElement == null) return;

        switch (_currentElement.ToolType)
        {
            case DrawingToolType.Pencil:
            case DrawingToolType.Highlight:
                _currentElement.Points.Add(currentPoint);
                break;

            case DrawingToolType.Rectangle:
            case DrawingToolType.Ellipse:
                var minX = Math.Min(_currentElement.StartPoint.X, currentPoint.X);
                var minY = Math.Min(_currentElement.StartPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - _currentElement.StartPoint.X);
                var height = Math.Abs(currentPoint.Y - _currentElement.StartPoint.Y);
                _currentElement.Rect = new Rect(minX, minY, width, height);
                _currentElement.EndPoint = currentPoint;
                break;

            case DrawingToolType.Line:
            case DrawingToolType.Arrow:
                _currentElement.EndPoint = currentPoint;
                break;
        }

        InvalidateVisual();
    }

    public void EndDrawing()
    {
        if (!_isDrawing || _currentElement == null) return;

        _isDrawing = false;
        UndoStack.Push(_currentElement);
        _currentElement = null;
        InvalidateVisual();
    }

    public void CommitText(string text, Point position, double? fontSize = null, string? fontFamily = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var effFontSize = fontSize ?? CurrentFontSize;
        var effFontFamily = !string.IsNullOrWhiteSpace(fontFamily) ? fontFamily : CurrentFontFamily;

        var element = new DrawingElement
        {
            ToolType = DrawingToolType.Text,
            Color = CurrentColor,
            FontSize = effFontSize > 0 ? effFontSize : 16.0,
            FontFamily = effFontFamily,
            StartPoint = position,
            Text = text.Trim()
        };

        UndoStack.Push(element);
        InvalidateVisual();
    }

    public void DismissInlineEditor(bool commit = true)
    {
        if (_inlineEditor == null) return;

        var editor = _inlineEditor;
        var pos = _inlineEditorPosition;
        _inlineEditor = null;

        if (Children.Contains(editor))
        {
            Children.Remove(editor);
        }

        if (commit && !string.IsNullOrWhiteSpace(editor.Text))
        {
            CommitText(editor.Text, pos);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (CurrentTool == DrawingToolType.None) return;

        var pos = e.GetPosition(this);

        if (CurrentTool == DrawingToolType.Text)
        {
            DismissInlineEditor(commit: true);

            _inlineEditorPosition = pos;
            _inlineEditor = new TextBox
            {
                FontFamily = new FontFamily(CurrentFontFamily),
                FontSize = CurrentFontSize,
                Foreground = new SolidColorBrush(CurrentColor),
                Background = new SolidColorBrush(Color.FromArgb(180, 24, 24, 24)),
                BorderBrush = new SolidColorBrush(CurrentColor),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(4, 2, 4, 2),
                MinWidth = 90,
                AcceptsReturn = false
            };

            _inlineEditor.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    DismissInlineEditor(commit: true);
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    DismissInlineEditor(commit: false);
                    args.Handled = true;
                }
            };

            _inlineEditor.LostFocus += (s, args) =>
            {
                DismissInlineEditor(commit: true);
            };

            Children.Add(_inlineEditor);
            Canvas.SetLeft(_inlineEditor, pos.X);
            Canvas.SetTop(_inlineEditor, pos.Y);
            _inlineEditor.Focus();
            e.Handled = true;
            return;
        }

        CaptureMouse();
        StartDrawing(pos);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isDrawing || _currentElement == null) return;

        MoveDrawing(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_isDrawing || _currentElement == null) return;

        ReleaseMouseCapture();
        EndDrawing();
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
