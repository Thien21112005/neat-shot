using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeatShot.Presentation.Controls;

public partial class SelectionCanvas : UserControl
{
    private enum DragMode
    {
        None,
        Select,
        Move,
        ResizeNW,
        ResizeN,
        ResizeNE,
        ResizeW,
        ResizeE,
        ResizeSW,
        ResizeS,
        ResizeSE
    }

    private DragMode _dragMode = DragMode.None;
    private Point _startPoint;
    private CaptureRegion _originalRegion;

    public event EventHandler<Point>? RegionMoved;

    public static readonly DependencyProperty SelectedRegionProperty =
        DependencyProperty.Register(
            nameof(SelectedRegion),
            typeof(CaptureRegion),
            typeof(SelectionCanvas),
            new FrameworkPropertyMetadata(default(CaptureRegion), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedRegionChangedCallback));

    public CaptureRegion SelectedRegion
    {
        get => (CaptureRegion)GetValue(SelectedRegionProperty);
        set => SetValue(SelectedRegionProperty, value);
    }

    public SelectionCanvas()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        FullScreenGeometry.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
    }

    private static void OnSelectedRegionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectionCanvas canvas && e.NewValue is CaptureRegion region)
        {
            canvas.UpdateVisuals(region);
        }
    }

    private static bool IsNear(Point p, double targetX, double targetY, double distance = 10.0)
    {
        return Math.Abs(p.X - targetX) <= distance && Math.Abs(p.Y - targetY) <= distance;
    }

    private DragMode GetHitHandle(Point pos, Rect rect)
    {
        if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0) return DragMode.None;

        var midX = rect.Left + rect.Width / 2;
        var midY = rect.Top + rect.Height / 2;

        if (IsNear(pos, rect.Left, rect.Top)) return DragMode.ResizeNW;
        if (IsNear(pos, rect.Right, rect.Top)) return DragMode.ResizeNE;
        if (IsNear(pos, rect.Left, rect.Bottom)) return DragMode.ResizeSW;
        if (IsNear(pos, rect.Right, rect.Bottom)) return DragMode.ResizeSE;

        if (IsNear(pos, midX, rect.Top)) return DragMode.ResizeN;
        if (IsNear(pos, midX, rect.Bottom)) return DragMode.ResizeS;
        if (IsNear(pos, rect.Left, midY)) return DragMode.ResizeW;
        if (IsNear(pos, rect.Right, midY)) return DragMode.ResizeE;

        return DragMode.None;
    }

    private static Cursor GetCursorForMode(DragMode mode) => mode switch
    {
        DragMode.ResizeNW => Cursors.SizeNWSE,
        DragMode.ResizeSE => Cursors.SizeNWSE,
        DragMode.ResizeNE => Cursors.SizeNESW,
        DragMode.ResizeSW => Cursors.SizeNESW,
        DragMode.ResizeN => Cursors.SizeNS,
        DragMode.ResizeS => Cursors.SizeNS,
        DragMode.ResizeW => Cursors.SizeWE,
        DragMode.ResizeE => Cursors.SizeWE,
        DragMode.Move => Cursors.SizeAll,
        _ => CursorHelper.WhiteCrosshair
    };

    private void UpdateHoverCursor(Point pos)
    {
        if (SelectedRegion.IsValid)
        {
            var rect = SelectedRegion.ToRect();
            var handle = GetHitHandle(pos, rect);
            if (handle != DragMode.None)
            {
                Cursor = GetCursorForMode(handle);
                return;
            }

            if (rect.Contains(pos))
            {
                Cursor = Cursors.SizeAll;
                return;
            }
        }

        Cursor = CursorHelper.WhiteCrosshair;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var pos = e.GetPosition(this);

        if (SelectedRegion.IsValid)
        {
            var rect = SelectedRegion.ToRect();
            var handle = GetHitHandle(pos, rect);
            if (handle != DragMode.None)
            {
                _dragMode = handle;
                _startPoint = pos;
                _originalRegion = SelectedRegion;
                CaptureMouse();
                Cursor = GetCursorForMode(handle);
                e.Handled = true;
                return;
            }

            if (rect.Contains(pos))
            {
                _dragMode = DragMode.Move;
                _startPoint = pos;
                _originalRegion = SelectedRegion;
                CaptureMouse();
                Cursor = Cursors.SizeAll;
                e.Handled = true;
                return;
            }
        }

        // Bấm ngoài vùng chọn hoặc chưa có vùng chọn -> Bắt đầu vẽ vùng chọn mới
        _dragMode = DragMode.Select;
        _startPoint = pos;
        CaptureMouse();
        SelectedRegion = new CaptureRegion(pos.X, pos.Y, 0, 0);
        UpdateVisuals(SelectedRegion);
        Cursor = CursorHelper.WhiteCrosshair;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var pos = e.GetPosition(this);

        if (_dragMode == DragMode.Move)
        {
            var dx = pos.X - _startPoint.X;
            var dy = pos.Y - _startPoint.Y;

            var newX = _originalRegion.X + dx;
            var newY = _originalRegion.Y + dy;

            // Giữ vùng chọn bên trong màn hình
            newX = Math.Max(0, Math.Min(ActualWidth - _originalRegion.Width, newX));
            newY = Math.Max(0, Math.Min(ActualHeight - _originalRegion.Height, newY));

            var prevX = SelectedRegion.X;
            var prevY = SelectedRegion.Y;

            SelectedRegion = new CaptureRegion(newX, newY, _originalRegion.Width, _originalRegion.Height);
            UpdateVisuals(SelectedRegion);

            var actualDx = newX - prevX;
            var actualDy = newY - prevY;
            if (Math.Abs(actualDx) > 0.001 || Math.Abs(actualDy) > 0.001)
            {
                RegionMoved?.Invoke(this, new Point(actualDx, actualDy));
            }

            e.Handled = true;
            return;
        }

        if (_dragMode >= DragMode.ResizeNW && _dragMode <= DragMode.ResizeSE)
        {
            var dx = pos.X - _startPoint.X;
            var dy = pos.Y - _startPoint.Y;

            var left = _originalRegion.X;
            var top = _originalRegion.Y;
            var right = _originalRegion.X + _originalRegion.Width;
            var bottom = _originalRegion.Y + _originalRegion.Height;

            switch (_dragMode)
            {
                case DragMode.ResizeNW: left += dx; top += dy; break;
                case DragMode.ResizeN: top += dy; break;
                case DragMode.ResizeNE: right += dx; top += dy; break;
                case DragMode.ResizeW: left += dx; break;
                case DragMode.ResizeE: right += dx; break;
                case DragMode.ResizeSW: left += dx; bottom += dy; break;
                case DragMode.ResizeS: bottom += dy; break;
                case DragMode.ResizeSE: right += dx; bottom += dy; break;
            }

            left = Math.Max(0, Math.Min(ActualWidth, left));
            top = Math.Max(0, Math.Min(ActualHeight, top));
            right = Math.Max(0, Math.Min(ActualWidth, right));
            bottom = Math.Max(0, Math.Min(ActualHeight, bottom));

            var region = CaptureRegion.FromPoints(new Point(left, top), new Point(right, bottom));
            SelectedRegion = region;
            UpdateVisuals(region);
            e.Handled = true;
            return;
        }

        if (_dragMode == DragMode.Select)
        {
            var region = CaptureRegion.FromPoints(_startPoint, pos);
            SelectedRegion = region;
            UpdateVisuals(region);
            e.Handled = true;
            return;
        }

        UpdateHoverCursor(pos);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_dragMode != DragMode.None)
        {
            if (_dragMode == DragMode.Select)
            {
                // Nếu kích thước vùng chọn quá nhỏ (< 5px), coi như click nhầm và xóa vùng chọn
                if (SelectedRegion.Width < 5 || SelectedRegion.Height < 5)
                {
                    SelectedRegion = CaptureRegion.Empty;
                    UpdateVisuals(SelectedRegion);
                }
            }

            _dragMode = DragMode.None;
            ReleaseMouseCapture();
            UpdateHoverCursor(e.GetPosition(this));
            e.Handled = true;
        }
    }

    private void UpdateVisuals(CaptureRegion region)
    {
        if (region.IsValid)
        {
            var rect = region.ToRect();
            SelectionGeometry.Rect = rect;

            // Cập nhật vị trí và kích thước viền
            SelectionBorder.Visibility = Visibility.Visible;
            SelectionBorder.Width = rect.Width;
            SelectionBorder.Height = rect.Height;
            Canvas.SetLeft(SelectionBorder, rect.Left);
            Canvas.SetTop(SelectionBorder, rect.Top);

            // Cập nhật nhãn kích thước W x H
            DimensionBadge.Visibility = Visibility.Visible;
            DimensionText.Text = $"{(int)rect.Width} × {(int)rect.Height}";

            var badgeLeft = rect.Left;
            var badgeTop = rect.Top - 24;
            if (badgeTop < 0) badgeTop = rect.Top + 6;

            Canvas.SetLeft(DimensionBadge, Math.Max(0, badgeLeft));
            Canvas.SetTop(DimensionBadge, Math.Max(0, badgeTop));

            // Cập nhật 8 nút điều chỉnh kích thước (Handles)
            HandlesLayer.Visibility = Visibility.Visible;
            var halfW = rect.Width / 2;
            var halfH = rect.Height / 2;

            Canvas.SetLeft(HandleNW, rect.Left - 4); Canvas.SetTop(HandleNW, rect.Top - 4);
            Canvas.SetLeft(HandleN, rect.Left + halfW - 4); Canvas.SetTop(HandleN, rect.Top - 4);
            Canvas.SetLeft(HandleNE, rect.Right - 4); Canvas.SetTop(HandleNE, rect.Top - 4);
            Canvas.SetLeft(HandleW, rect.Left - 4); Canvas.SetTop(HandleW, rect.Top + halfH - 4);
            Canvas.SetLeft(HandleE, rect.Right - 4); Canvas.SetTop(HandleE, rect.Top + halfH - 4);
            Canvas.SetLeft(HandleSW, rect.Left - 4); Canvas.SetTop(HandleSW, rect.Bottom - 4);
            Canvas.SetLeft(HandleS, rect.Left + halfW - 4); Canvas.SetTop(HandleS, rect.Bottom - 4);
            Canvas.SetLeft(HandleSE, rect.Right - 4); Canvas.SetTop(HandleSE, rect.Bottom - 4);
        }
        else
        {
            SelectionGeometry.Rect = Rect.Empty;
            SelectionBorder.Visibility = Visibility.Collapsed;
            DimensionBadge.Visibility = Visibility.Collapsed;
            HandlesLayer.Visibility = Visibility.Collapsed;
        }
    }
}
