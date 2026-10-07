using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeatShot.Presentation.Controls;

public partial class SelectionCanvas : UserControl
{
    private Point _startPoint;
    private bool _isDragging;

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

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        _startPoint = e.GetPosition(this);
        _isDragging = true;
        CaptureMouse();

        SelectedRegion = new CaptureRegion(_startPoint.X, _startPoint.Y, 0, 0);
        UpdateVisuals(SelectedRegion);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isDragging) return;

        var currentPoint = e.GetPosition(this);
        var region = CaptureRegion.FromPoints(_startPoint, currentPoint);

        SelectedRegion = region;
        UpdateVisuals(region);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
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
        }
        else
        {
            SelectionGeometry.Rect = Rect.Empty;
            SelectionBorder.Visibility = Visibility.Collapsed;
            DimensionBadge.Visibility = Visibility.Collapsed;
        }
    }
}
