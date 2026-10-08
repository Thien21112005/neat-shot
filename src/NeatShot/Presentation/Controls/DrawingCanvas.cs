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

    /// <summary>
    /// Đối tượng vẽ hiện đang được chọn (khi sử dụng công cụ Select hoặc click chọn).
    /// </summary>
    public DrawingElement? SelectedElement { get; private set; }

    /// <summary>
    /// Phát ra khi con trỏ chuột di chuyển trong chế độ hút màu (Eyedropper).
    /// </summary>
    public event EventHandler<Point>? EyedropperHovered;

    /// <summary>
    /// Phát ra khi người dùng click chuột để chọn màu trong chế độ hút màu (Eyedropper).
    /// </summary>
    public event EventHandler<Point>? EyedropperClicked;

    /// <summary>
    /// Kích hoạt sự kiện di chuột trong chế độ hút màu tại toạ độ chỉ định (phục vụ test).
    /// </summary>
    public void HoverEyedropperAt(Point pos)
    {
        EyedropperHovered?.Invoke(this, pos);
    }

    /// <summary>
    /// Kích hoạt sự kiện click chọn màu trong chế độ hút màu tại toạ độ chỉ định (phục vụ test).
    /// </summary>
    public void SampleEyedropperAt(Point pos)
    {
        EyedropperClicked?.Invoke(this, pos);
    }

    private bool _isDraggingSelected;
    private Point _lastDragPoint;
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
        if (CurrentTool == DrawingToolType.None || CurrentTool == DrawingToolType.Eyedropper) return;

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

    /// <summary>
    /// Lựa chọn phần tử vẽ tại toạ độ chỉ định (duyệt từ trên xuống dưới theo thứ tự vẽ).
    /// </summary>
    public bool SelectElementAt(Point pos)
    {
        for (int i = UndoStack.Items.Count - 1; i >= 0; i--)
        {
            var item = UndoStack.Items[i];
            if (HitTestHelper.HitTest(item, pos))
            {
                SelectedElement = item;
                InvalidateVisual();
                return true;
            }
        }

        SelectedElement = null;
        InvalidateVisual();
        return false;
    }

    /// <summary>
    /// Bỏ chọn phần tử hiện tại.
    /// </summary>
    public void ClearSelection()
    {
        if (SelectedElement != null)
        {
            SelectedElement = null;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Dịch chuyển phần tử đang chọn theo khoảng cách (dx, dy).
    /// </summary>
    public void MoveSelectedElement(double dx, double dy)
    {
        if (SelectedElement == null) return;
        if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001) return;

        SelectedElement.StartPoint = new Point(SelectedElement.StartPoint.X + dx, SelectedElement.StartPoint.Y + dy);
        SelectedElement.EndPoint = new Point(SelectedElement.EndPoint.X + dx, SelectedElement.EndPoint.Y + dy);

        if (!SelectedElement.Rect.IsEmpty)
        {
            SelectedElement.Rect = new Rect(
                SelectedElement.Rect.X + dx,
                SelectedElement.Rect.Y + dy,
                SelectedElement.Rect.Width,
                SelectedElement.Rect.Height);
        }

        if (SelectedElement.Points != null)
        {
            for (int i = 0; i < SelectedElement.Points.Count; i++)
            {
                SelectedElement.Points[i] = new Point(SelectedElement.Points[i].X + dx, SelectedElement.Points[i].Y + dy);
            }
        }

        InvalidateVisual();
    }

    /// <summary>
    /// Xoá phần tử đang được chọn khỏi UndoStack.
    /// </summary>
    public bool DeleteSelectedElement()
    {
        if (SelectedElement == null) return false;

        var target = SelectedElement;
        SelectedElement = null;

        var items = UndoStack.Items.ToList();
        if (items.Remove(target))
        {
            UndoStack.Clear();
            foreach (var it in items)
            {
                UndoStack.Push(it);
            }
            InvalidateVisual();
            return true;
        }

        InvalidateVisual();
        return false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (CurrentTool == DrawingToolType.None) return;

        var pos = e.GetPosition(this);

        if (CurrentTool == DrawingToolType.Eyedropper)
        {
            EyedropperClicked?.Invoke(this, pos);
            e.Handled = true;
            return;
        }

        if (CurrentTool == DrawingToolType.Select)
        {
            if (SelectElementAt(pos))
            {
                _isDraggingSelected = true;
                _lastDragPoint = pos;
                CaptureMouse();
            }
            e.Handled = true;
            return;
        }

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

        var currentPoint = e.GetPosition(this);

        if (CurrentTool == DrawingToolType.Eyedropper)
        {
            EyedropperHovered?.Invoke(this, currentPoint);
            return;
        }

        if (CurrentTool == DrawingToolType.Select)
        {
            if (_isDraggingSelected && SelectedElement != null)
            {
                var dx = currentPoint.X - _lastDragPoint.X;
                var dy = currentPoint.Y - _lastDragPoint.Y;
                _lastDragPoint = currentPoint;
                MoveSelectedElement(dx, dy);
                Cursor = Cursors.SizeAll;
            }
            else
            {
                var isHovering = UndoStack.Items.Any(item => HitTestHelper.HitTest(item, currentPoint));
                Cursor = isHovering ? Cursors.SizeAll : Cursors.Hand;
            }
            e.Handled = true;
            return;
        }

        if (!_isDrawing || _currentElement == null) return;

        MoveDrawing(currentPoint);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (CurrentTool == DrawingToolType.Select)
        {
            if (_isDraggingSelected)
            {
                _isDraggingSelected = false;
                ReleaseMouseCapture();
            }
            e.Handled = true;
            return;
        }

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

        // 3. Nếu đang có đối tượng được chọn, vẽ khung viền nét đứt báo hiệu
        if (SelectedElement != null)
        {
            var box = SelectedElement.GetBoundingBox();
            if (!box.IsEmpty)
            {
                var adRect = new Rect(box.X - 4, box.Y - 4, Math.Max(8, box.Width + 8), Math.Max(8, box.Height + 8));
                var dashPen = new Pen(new SolidColorBrush(Color.FromRgb(0, 120, 212)), 1.5)
                {
                    DashStyle = DashStyles.Dash
                };
                dashPen.Freeze();
                dc.DrawRectangle(null, dashPen, adRect);

                var handleBrush = Brushes.White;
                var handlePen = new Pen(new SolidColorBrush(Color.FromRgb(0, 120, 212)), 1.0);
                handlePen.Freeze();
                dc.DrawEllipse(handleBrush, handlePen, new Point(adRect.Left, adRect.Top), 3, 3);
                dc.DrawEllipse(handleBrush, handlePen, new Point(adRect.Right, adRect.Top), 3, 3);
                dc.DrawEllipse(handleBrush, handlePen, new Point(adRect.Left, adRect.Bottom), 3, 3);
                dc.DrawEllipse(handleBrush, handlePen, new Point(adRect.Right, adRect.Bottom), 3, 3);
            }
        }
    }
}
