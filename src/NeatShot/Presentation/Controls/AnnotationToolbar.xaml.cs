using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

public partial class AnnotationToolbar : UserControl
{
    private DrawingToolType _activeTool = DrawingToolType.None;
    private string _selectedColorHex = "#FFE81123";
    private Color _selectedColor = Color.FromRgb(0xE8, 0x11, 0x23);

    /// <summary>
    /// Quản lý lịch sử các màu đã chọn hoặc hút gần đây (tối đa 6 màu).
    /// </summary>
    public ColorHistory History { get; } = new(capacity: 6);

    /// <summary>
    /// Công cụ vẽ hiện đang được kích hoạt.
    /// </summary>
    public DrawingToolType ActiveTool => _activeTool;

    /// <summary>
    /// Màu sắc hiện đang được chọn.
    /// </summary>
    public Color SelectedColor => _selectedColor;

    public string SelectedFontFamily { get; private set; } = "Segoe UI";
    public double SelectedFontSize { get; private set; } = 16.0;
    public double SelectedStepRadius { get; private set; } = 14.0;

    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler<string>? FontFamilyChanged;
    public event EventHandler<double>? FontSizeChanged;
    public event EventHandler<double>? StepSizeChanged;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? CopyRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CloseRequested;
    public event EventHandler? PinRequested;
    public event EventHandler? OcrRequested;
    public event EventHandler<BeautifyOptions?>? BeautifyChanged;
    public event EventHandler<Point>? ToolbarMoved;
    public event EventHandler? ToolbarResetPosition;

    /// <summary>
    /// Các tùy chọn làm đẹp ảnh đang được người dùng lựa chọn (null nếu tắt).
    /// </summary>
    public BeautifyOptions? SelectedBeautifyOptions { get; private set; }

    private bool _isDragPending;
    private bool _isDraggingToolbar;
    private Point _dragStartPoint;
    private Point _dragLastPoint;
    private bool _isUpdatingSelection;
    private bool _isInitialized;
    private Rect _lastRegionRect;
    private Size _lastScreenSize;

    public AnnotationToolbar()
    {
        InitializeComponent();
        _isInitialized = true;
        SetupStepperListeners();
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
    }

    private void OnDragGripMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToolbarResetPosition?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragPending = true;
            _isDraggingToolbar = false;
            _dragStartPoint = e.GetPosition(this);
            _dragLastPoint = _dragStartPoint;
            DragGrip.CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnDragGripMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragPending && !_isDraggingToolbar) return;

        var cur = e.GetPosition(this);

        if (_isDragPending && !_isDraggingToolbar)
        {
            var minX = Math.Max(4.0, SystemParameters.MinimumHorizontalDragDistance);
            var minY = Math.Max(4.0, SystemParameters.MinimumVerticalDragDistance);

            if (Math.Abs(cur.X - _dragStartPoint.X) >= minX || Math.Abs(cur.Y - _dragStartPoint.Y) >= minY)
            {
                _isDraggingToolbar = true;
                _dragLastPoint = cur;
            }
            else
            {
                return;
            }
        }

        if (_isDraggingToolbar && VerticalBar != null)
        {
            var deltaX = cur.X - _dragLastPoint.X;
            var deltaY = cur.Y - _dragLastPoint.Y;
            _dragLastPoint = cur;

            MoveVerticalBarDelta(deltaX, deltaY);
            e.Handled = true;
        }
    }

    private void OnDragGripMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragPending = false;
        _isDraggingToolbar = false;
        if (DragGrip.IsMouseCaptured)
        {
            DragGrip.ReleaseMouseCapture();
        }
        e.Handled = true;
    }

    internal void MoveVerticalBarDelta(double deltaX, double deltaY)
    {
        if (VerticalBar == null) return;
        var curLeft = Canvas.GetLeft(VerticalBar);
        var curTop = Canvas.GetTop(VerticalBar);
        if (double.IsNaN(curLeft)) curLeft = 0;
        if (double.IsNaN(curTop)) curTop = 0;

        var screenSize = _lastScreenSize.Width > 0
            ? _lastScreenSize
            : new Size(ActualWidth > 0 ? ActualWidth : 1920, ActualHeight > 0 ? ActualHeight : 1080);
        var vWidth = VerticalBar.ActualWidth > 0 ? VerticalBar.ActualWidth : 42;
        var vHeight = VerticalBar.ActualHeight > 0 ? VerticalBar.ActualHeight : 360;

        var newLeft = Math.Clamp(curLeft + deltaX, 4, Math.Max(4, screenSize.Width - vWidth - 4));
        var newTop = Math.Clamp(curTop + deltaY, 4, Math.Max(4, screenSize.Height - vHeight - 4));

        Canvas.SetLeft(VerticalBar, newLeft);
        Canvas.SetTop(VerticalBar, newTop);
        UpdateFlyoutPosition();

        ToolbarMoved?.Invoke(this, new Point(newLeft, newTop));
    }

    /// <summary>
    /// Đặt lại toàn bộ trạng thái công cụ về None.
    /// </summary>
    public void ResetTools()
    {
        _activeTool = DrawingToolType.None;
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
        ToolSelected?.Invoke(this, DrawingToolType.None);
    }

    /// <summary>
    /// Kích hoạt một công cụ cụ thể từ bên ngoài (ví dụ sau khi kết thúc Eyedropper hoặc phím tắt).
    /// </summary>
    public void SetActiveTool(DrawingToolType tool)
    {
        _activeTool = tool;
        _isUpdatingSelection = true;
        try
        {
            if (tool == DrawingToolType.Rectangle && ShapeComboBox != null) ShapeComboBox.SelectedIndex = 0;
            else if (tool == DrawingToolType.Ellipse && ShapeComboBox != null) ShapeComboBox.SelectedIndex = 1;
            else if (tool == DrawingToolType.Arrow && LineComboBox != null) LineComboBox.SelectedIndex = 0;
            else if (tool == DrawingToolType.Line && LineComboBox != null) LineComboBox.SelectedIndex = 1;
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
    }

    /// <summary>
    /// Chọn công cụ hình khối (Rectangle hoặc Ellipse) từ dropdown hoặc phím tắt.
    /// </summary>
    public void SelectShapeTool(DrawingToolType shapeTool)
    {
        if (shapeTool != DrawingToolType.Rectangle && shapeTool != DrawingToolType.Ellipse) return;

        _activeTool = shapeTool;
        _isUpdatingSelection = true;
        try
        {
            if (ShapeComboBox != null)
            {
                ShapeComboBox.SelectedIndex = shapeTool == DrawingToolType.Rectangle ? 0 : 1;
            }
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
        ToolSelected?.Invoke(this, _activeTool);
    }

    /// <summary>
    /// Chọn công cụ đường kẻ (Arrow hoặc Line) từ dropdown hoặc phím tắt.
    /// </summary>
    public void SelectLineTool(DrawingToolType lineTool)
    {
        if (lineTool != DrawingToolType.Arrow && lineTool != DrawingToolType.Line) return;

        _activeTool = lineTool;
        _isUpdatingSelection = true;
        try
        {
            if (LineComboBox != null)
            {
                LineComboBox.SelectedIndex = lineTool == DrawingToolType.Arrow ? 0 : 1;
            }
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
        ToolSelected?.Invoke(this, _activeTool);
    }

    /// <summary>
    /// Chọn phông chữ cho công cụ Text.
    /// </summary>
    public void SelectFontFamily(string fontFamily)
    {
        if (string.IsNullOrWhiteSpace(fontFamily)) return;
        SelectedFontFamily = fontFamily;
        if (FontFamilyComboBox != null)
        {
            foreach (ComboBoxItem item in FontFamilyComboBox.Items)
            {
                if (string.Equals(item.Content?.ToString(), fontFamily, StringComparison.OrdinalIgnoreCase))
                {
                    FontFamilyComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        FontFamilyChanged?.Invoke(this, fontFamily);
    }

    /// <summary>
    /// Chọn cỡ chữ cho công cụ Text.
    /// </summary>
    public void SelectFontSize(double fontSize)
    {
        if (fontSize <= 0) return;
        SelectedFontSize = Math.Clamp(fontSize, 8.0, 96.0);
        if (FontSizeComboBox != null)
        {
            _isUpdatingSelection = true;
            try
            {
                FontSizeComboBox.Text = SelectedFontSize.ToString("0.#");
                foreach (ComboBoxItem item in FontSizeComboBox.Items)
                {
                    if (double.TryParse(item.Content?.ToString(), out var sz) && Math.Abs(sz - SelectedFontSize) < 0.1)
                    {
                        FontSizeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
        FontSizeChanged?.Invoke(this, SelectedFontSize);
    }

    /// <summary>
    /// Chọn kích thước cho công cụ đánh số bước StepCounter.
    /// </summary>
    public void SelectStepSize(double radius)
    {
        if (radius <= 0) return;
        SelectedStepRadius = Math.Clamp(radius, 6.0, 60.0);
        if (StepSizeComboBox != null)
        {
            _isUpdatingSelection = true;
            try
            {
                StepSizeComboBox.Text = SelectedStepRadius.ToString("0.#");
                foreach (ComboBoxItem item in StepSizeComboBox.Items)
                {
                    if (double.TryParse(item.Tag?.ToString() ?? item.Content?.ToString(), out var r) && Math.Abs(r - SelectedStepRadius) < 0.1)
                    {
                        StepSizeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
        StepSizeChanged?.Invoke(this, SelectedStepRadius);
    }

    private void OnStepSizeDecreaseClick(object sender, RoutedEventArgs e)
    {
        SelectStepSize(Math.Max(6.0, SelectedStepRadius - 1.0));
    }

    private void OnStepSizeIncreaseClick(object sender, RoutedEventArgs e)
    {
        SelectStepSize(Math.Min(60.0, SelectedStepRadius + 1.0));
    }

    private void OnFontSizeDecreaseClick(object sender, RoutedEventArgs e)
    {
        SelectFontSize(Math.Max(8.0, SelectedFontSize - 1.0));
    }

    private void OnFontSizeIncreaseClick(object sender, RoutedEventArgs e)
    {
        SelectFontSize(Math.Min(96.0, SelectedFontSize + 1.0));
    }

    private void OnStepSizeMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            SelectStepSize(Math.Min(60.0, SelectedStepRadius + 1.0));
        }
        else if (e.Delta < 0)
        {
            SelectStepSize(Math.Max(6.0, SelectedStepRadius - 1.0));
        }
        e.Handled = true;
    }

    private void OnFontSizeMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            SelectFontSize(Math.Min(96.0, SelectedFontSize + 1.0));
        }
        else if (e.Delta < 0)
        {
            SelectFontSize(Math.Max(8.0, SelectedFontSize - 1.0));
        }
        e.Handled = true;
    }

    private void OnStepSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (StepSizeComboBox?.SelectedItem is ComboBoxItem item &&
            double.TryParse(item.Tag?.ToString() ?? item.Content?.ToString(), out var radius))
        {
            SelectStepSize(radius);
        }
    }

    /// <summary>
    /// Chọn màu hiện tại và kích hoạt sự kiện ColorSelected.
    /// </summary>
    public void SelectColor(Color color)
    {
        _selectedColor = color;
        _selectedColorHex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

        // Đóng các dropdown nếu đang mở để tránh giữ capture chuột
        if (ShapeComboBox != null && ShapeComboBox.IsDropDownOpen) ShapeComboBox.IsDropDownOpen = false;
        if (LineComboBox != null && LineComboBox.IsDropDownOpen) LineComboBox.IsDropDownOpen = false;

        // Nếu người dùng chọn màu khi chưa có công cụ vẽ nào bật, tự động kích hoạt bút vẽ (Pencil)
        if (_activeTool == DrawingToolType.None || _activeTool == DrawingToolType.Select || _activeTool == DrawingToolType.Eyedropper)
        {
            _activeTool = DrawingToolType.Pencil;
            UpdateToolButtonVisuals();
            ToolSelected?.Invoke(this, _activeTool);
        }

        UpdateColorButtonVisuals();
        ColorSelected?.Invoke(this, color);
    }

    /// <summary>
    /// Thêm màu mới vào danh sách lịch sử màu và tự động chọn màu đó.
    /// </summary>
    public void AddColorToHistory(Color color)
    {
        History.AddColor(color);
        SelectColor(color);
    }

    /// <summary>
    /// Nạp danh sách lịch sử màu sắc từ mảng mã màu Hex đã lưu.
    /// </summary>
    public void LoadColorHistory(IEnumerable<string>? hexColors)
    {
        if (hexColors == null) return;
        History.Clear();
        var list = hexColors.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
        // Duyệt ngược để phần tử đầu tiên (gần nhất) kết thúc ở index 0 của History.Colors
        for (int i = list.Count - 1; i >= 0; i--)
        {
            try
            {
                var color = ColorHelper.FromHex(list[i]);
                History.AddColor(color);
            }
            catch
            {
                // Bỏ qua mã màu không hợp lệ
            }
        }
        UpdateColorButtonVisuals();
    }

    /// <summary>
    /// Lấy danh sách mã Hex của các màu hiện có trong lịch sử màu.
    /// </summary>
    public List<string> GetColorHistoryHex()
    {
        return History.Colors
            .Select(c => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}")
            .ToList();
    }

    private void ToggleTool(DrawingToolType tool)
    {
        _activeTool = _activeTool == tool ? DrawingToolType.None : tool;
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
        ToolSelected?.Invoke(this, _activeTool);
    }

    private void UpdateToolButtonVisuals()
    {
        SetButtonActive(SelectButton, _activeTool == DrawingToolType.Select);
        SetButtonActive(PencilButton, _activeTool == DrawingToolType.Pencil);
        SetButtonActive(RectButton, _activeTool == DrawingToolType.Rectangle);
        SetButtonActive(EllipseButton, _activeTool == DrawingToolType.Ellipse);
        SetButtonActive(LineButton, _activeTool == DrawingToolType.Line);
        SetButtonActive(ArrowButton, _activeTool == DrawingToolType.Arrow);
        SetButtonActive(HighlightButton, _activeTool == DrawingToolType.Highlight);
        SetButtonActive(TextButton, _activeTool == DrawingToolType.Text);
        SetButtonActive(PixelateButton, _activeTool == DrawingToolType.Pixelate);
        SetButtonActive(BlurButton, _activeTool == DrawingToolType.Blur);
        SetButtonActive(StepCounterButton, _activeTool == DrawingToolType.StepCounter);
        SetButtonActive(EyedropperButton, _activeTool == DrawingToolType.Eyedropper);

        SetControlActive(ShapeComboBox, _activeTool == DrawingToolType.Rectangle || _activeTool == DrawingToolType.Ellipse);
        SetControlActive(LineComboBox, _activeTool == DrawingToolType.Arrow || _activeTool == DrawingToolType.Line);

        var isFontActive = _activeTool == DrawingToolType.Text;
        var isStepActive = _activeTool == DrawingToolType.StepCounter;

        if (FontControlsPanel != null)
        {
            FontControlsPanel.Visibility = isFontActive ? Visibility.Visible : Visibility.Collapsed;
        }

        if (StepSizeControlsPanel != null)
        {
            StepSizeControlsPanel.Visibility = isStepActive ? Visibility.Visible : Visibility.Collapsed;
        }

        if (FlyoutOptionsPanel != null)
        {
            FlyoutOptionsPanel.Visibility = (isFontActive || isStepActive) ? Visibility.Visible : Visibility.Collapsed;
            if (FlyoutOptionsPanel.Visibility == Visibility.Visible)
            {
                UpdateFlyoutPosition();
            }
        }

        if (ColorPalettePanel != null)
        {
            ColorPalettePanel.Visibility = IsColorToolActive()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private static void SetButtonActive(Button? btn, bool isActive)
    {
        if (btn == null) return;
        if (isActive)
        {
            btn.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            btn.BorderBrush = new SolidColorBrush(Colors.White);
            btn.BorderThickness = new Thickness(1.5);
        }
        else
        {
            btn.ClearValue(Button.BackgroundProperty);
            btn.ClearValue(Button.BorderBrushProperty);
            btn.ClearValue(Button.BorderThicknessProperty);
        }
    }

    private static void SetControlActive(Control? ctrl, bool isActive)
    {
        if (ctrl == null) return;
        if (isActive)
        {
            ctrl.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            ctrl.BorderBrush = new SolidColorBrush(Colors.White);
            ctrl.BorderThickness = new Thickness(1.5);
        }
        else
        {
            ctrl.ClearValue(Control.BackgroundProperty);
            ctrl.ClearValue(Control.BorderBrushProperty);
            ctrl.ClearValue(Control.BorderThicknessProperty);
        }
    }

    private void OnSelectClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Select);
    private void OnPencilClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Pencil);
    private void OnRectClick(object sender, RoutedEventArgs e) => SelectShapeTool(DrawingToolType.Rectangle);
    private void OnEllipseClick(object sender, RoutedEventArgs e) => SelectShapeTool(DrawingToolType.Ellipse);
    private void OnLineClick(object sender, RoutedEventArgs e) => SelectLineTool(DrawingToolType.Line);
    private void OnArrowClick(object sender, RoutedEventArgs e) => SelectLineTool(DrawingToolType.Arrow);
    private void OnHighlightClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Highlight);
    private void OnTextClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Text);
    private void OnPixelateClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Pixelate);
    private void OnBlurClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Blur);
    private void OnStepCounterClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.StepCounter);
    private void OnEyedropperClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Eyedropper);

    private void OnShapeComboBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            if (ShapeComboBox != null)
            {
                ShapeComboBox.IsDropDownOpen = !ShapeComboBox.IsDropDownOpen;
                e.Handled = true;
            }
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            var pos = e.GetPosition(ShapeComboBox);
            var isArrowCorner = pos.X >= 22 && pos.Y >= 18;

            if (!isArrowCorner)
            {
                if (_activeTool == DrawingToolType.Rectangle || _activeTool == DrawingToolType.Ellipse)
                {
                    ResetTools();
                }
                else
                {
                    var tool = ShapeComboBox.SelectedIndex == 1 ? DrawingToolType.Ellipse : DrawingToolType.Rectangle;
                    SelectShapeTool(tool);
                }
                ShapeComboBox.IsDropDownOpen = false;
                e.Handled = true;
            }
        }
    }

    private void OnShapeItemPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Ellipse" ? DrawingToolType.Ellipse : DrawingToolType.Rectangle;
            SelectShapeTool(tool);
            if (ShapeComboBox != null) ShapeComboBox.IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private void OnShapeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (ShapeComboBox?.SelectedItem is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Ellipse" ? DrawingToolType.Ellipse : DrawingToolType.Rectangle;
            SelectShapeTool(tool);
            ShapeComboBox.IsDropDownOpen = false;
        }
    }

    private void OnShapeComboBoxDropDownOpened(object? sender, EventArgs e)
    {
        if (!_isInitialized) return;
        if (_activeTool != DrawingToolType.Rectangle && _activeTool != DrawingToolType.Ellipse)
        {
            if (ShapeComboBox?.SelectedItem is ComboBoxItem item)
            {
                var tool = item.Tag?.ToString() == "Ellipse" ? DrawingToolType.Ellipse : DrawingToolType.Rectangle;
                SelectShapeTool(tool);
            }
        }
    }

    private void OnLineComboBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            if (LineComboBox != null)
            {
                LineComboBox.IsDropDownOpen = !LineComboBox.IsDropDownOpen;
                e.Handled = true;
            }
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            var pos = e.GetPosition(LineComboBox);
            var isArrowCorner = pos.X >= 22 && pos.Y >= 18;

            if (!isArrowCorner)
            {
                if (_activeTool == DrawingToolType.Arrow || _activeTool == DrawingToolType.Line)
                {
                    ResetTools();
                }
                else
                {
                    var tool = LineComboBox.SelectedIndex == 1 ? DrawingToolType.Line : DrawingToolType.Arrow;
                    SelectLineTool(tool);
                }
                LineComboBox.IsDropDownOpen = false;
                e.Handled = true;
            }
        }
    }

    private void OnLineItemPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Line" ? DrawingToolType.Line : DrawingToolType.Arrow;
            SelectLineTool(tool);
            if (LineComboBox != null) LineComboBox.IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private void OnLineComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (LineComboBox?.SelectedItem is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Line" ? DrawingToolType.Line : DrawingToolType.Arrow;
            SelectLineTool(tool);
            LineComboBox.IsDropDownOpen = false;
        }
    }

    private void OnLineComboBoxDropDownOpened(object? sender, EventArgs e)
    {
        if (!_isInitialized) return;
        if (_activeTool != DrawingToolType.Arrow && _activeTool != DrawingToolType.Line)
        {
            if (LineComboBox?.SelectedItem is ComboBoxItem item)
            {
                var tool = item.Tag?.ToString() == "Line" ? DrawingToolType.Line : DrawingToolType.Arrow;
                SelectLineTool(tool);
            }
        }
    }

    private void OnFontFamilySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (FontFamilyComboBox?.SelectedItem is ComboBoxItem item && item.Content != null)
        {
            SelectedFontFamily = item.Content.ToString()!;
            FontFamilyChanged?.Invoke(this, SelectedFontFamily);
        }
    }

    private void OnFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (FontSizeComboBox?.SelectedItem is ComboBoxItem item &&
            double.TryParse(item.Content?.ToString(), out var size))
        {
            SelectFontSize(size);
        }
    }

    private void SetupStepperListeners()
    {
        if (StepSizeComboBox != null)
        {
            StepSizeComboBox.Text = SelectedStepRadius.ToString("0.#");
            DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(ComboBox))
                .AddValueChanged(StepSizeComboBox, (s, e) =>
                {
                    if (!_isInitialized || _isUpdatingSelection) return;
                    var text = StepSizeComboBox.Text.Trim();
                    if (double.TryParse(text, out var radius) && radius >= 6.0 && radius <= 60.0)
                    {
                        if (Math.Abs(radius - SelectedStepRadius) > 0.01)
                        {
                            SelectedStepRadius = radius;
                            StepSizeChanged?.Invoke(this, radius);
                        }
                    }
                });
        }

        if (FontSizeComboBox != null)
        {
            FontSizeComboBox.Text = SelectedFontSize.ToString("0.#");
            DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(ComboBox))
                .AddValueChanged(FontSizeComboBox, (s, e) =>
                {
                    if (!_isInitialized || _isUpdatingSelection) return;
                    var text = FontSizeComboBox.Text.Trim();
                    if (double.TryParse(text, out var size) && size >= 8.0 && size <= 96.0)
                    {
                        if (Math.Abs(size - SelectedFontSize) > 0.01)
                        {
                            SelectedFontSize = size;
                            FontSizeChanged?.Invoke(this, size);
                        }
                    }
                });
        }
    }

    private void OnColorSelect(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
        {
            var color = ColorHelper.FromHex(hex);
            SelectColor(color);
        }
    }

    private void OnHistoryColorClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Color color })
        {
            SelectColor(color);
        }
    }

    private bool IsColorToolActive()
    {
        return _activeTool is DrawingToolType.Pencil
            or DrawingToolType.Rectangle
            or DrawingToolType.Ellipse
            or DrawingToolType.Line
            or DrawingToolType.Arrow
            or DrawingToolType.Highlight
            or DrawingToolType.Text
            or DrawingToolType.StepCounter;
    }

    private void UpdateColorButtonVisuals()
    {
        if (!_isInitialized || ColorRedButton == null) return;

        if (ColorPalettePanel != null)
        {
            ColorPalettePanel.Visibility = IsColorToolActive()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        UpdateColorBorder(ColorRedButton);
        UpdateColorBorder(ColorYellowButton);
        UpdateColorBorder(ColorGreenButton);
        UpdateColorBorder(ColorBlueButton);
        UpdateColorBorder(ColorWhiteButton);
        UpdateHistorySwatches();
    }

    private void UpdateColorBorder(Button? btn)
    {
        if (btn == null) return;

        // CHỈ hiển thị viền chọn màu khi đang có công cụ vẽ dùng màu được kích hoạt
        var isSelected = IsColorToolActive() &&
                         btn.Tag is string hex &&
                         string.Equals(hex, _selectedColorHex, StringComparison.OrdinalIgnoreCase);

        btn.BorderBrush = isSelected ? Brushes.White : Brushes.Transparent;
        btn.BorderThickness = new Thickness(1.5);
    }

    private void UpdateHistorySwatches()
    {
        HistoryColorsPanel.Children.Clear();
        foreach (var col in History.Colors)
        {
            var isSelected = IsColorToolActive() &&
                             col.A == _selectedColor.A &&
                             col.R == _selectedColor.R &&
                             col.G == _selectedColor.G &&
                             col.B == _selectedColor.B;

            var btn = new Button
            {
                Width = 32,
                Height = 26,
                Margin = new Thickness(0, 1.5, 0, 1.5),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Tag = col,
                ToolTip = $"#{col.R:X2}{col.G:X2}{col.B:X2}",
                Background = Brushes.Transparent,
                BorderBrush = isSelected ? Brushes.White : Brushes.Transparent,
                BorderThickness = new Thickness(1.5),
                Content = new Border
                {
                    Width = 14,
                    Height = 14,
                    CornerRadius = new CornerRadius(7),
                    Background = new SolidColorBrush(col)
                }
            };

            if (TryFindResource("ColorButtonStyle") is Style colorStyle)
            {
                btn.Style = colorStyle;
            }

            btn.Click += OnHistoryColorClick;
            HistoryColorsPanel.Children.Add(btn);
        }
    }

    private void OnUndoClick(object sender, RoutedEventArgs e) => UndoRequested?.Invoke(this, EventArgs.Empty);
    private void OnRedoClick(object sender, RoutedEventArgs e) => RedoRequested?.Invoke(this, EventArgs.Empty);
    private void OnCopyClick(object sender, RoutedEventArgs e) => CopyRequested?.Invoke(this, EventArgs.Empty);
    private void OnSaveClick(object sender, RoutedEventArgs e) => SaveRequested?.Invoke(this, EventArgs.Empty);
    private void OnPinClick(object sender, RoutedEventArgs e) => PinRequested?.Invoke(this, EventArgs.Empty);
    private void OnOcrClick(object sender, RoutedEventArgs e) => OcrRequested?.Invoke(this, EventArgs.Empty);
    private void OnCloseClick(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void OnBeautifySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        if (BeautifyComboBox?.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag &&
            Enum.TryParse<BeautifyPreset>(tag, out var preset))
        {
            if (preset == BeautifyPreset.None)
            {
                SelectedBeautifyOptions = null;
            }
            else
            {
                SelectedBeautifyOptions = new BeautifyOptions
                {
                    IsEnabled = true,
                    Preset = preset,
                    Padding = 20.0,
                    CornerRadius = 12.0
                };
            }
            BeautifyChanged?.Invoke(this, SelectedBeautifyOptions);
        }
    }

    private void OnBeautifyButtonClick(object sender, RoutedEventArgs e)
    {
        if (SelectedBeautifyOptions == null || !SelectedBeautifyOptions.IsEnabled)
        {
            if (BeautifyComboBox != null) BeautifyComboBox.SelectedIndex = 1;
        }
        else
        {
            if (BeautifyComboBox != null) BeautifyComboBox.SelectedIndex = 0;
        }
    }

    public void SelectBeautifyPreset(BeautifyPreset preset)
    {
        if (BeautifyComboBox == null) return;
        var tag = preset.ToString();
        for (int i = 0; i < BeautifyComboBox.Items.Count; i++)
        {
            if (BeautifyComboBox.Items[i] is ComboBoxItem item &&
                string.Equals(item.Tag as string, tag, StringComparison.OrdinalIgnoreCase))
            {
                BeautifyComboBox.SelectedIndex = i;
                return;
            }
        }
    }

    /// <summary>
    /// Định vị 2 thanh công cụ (Thanh dọc vẽ ở cạnh phải, Thanh ngang tác vụ ở cạnh dưới)
    /// chuẩn phong cách Lightshot thích ứng theo toạ độ vùng chọn và kích thước màn hình.
    /// </summary>
    public void UpdatePositions(Rect regionRect, Size screenSize)
    {
        if (VerticalBar == null || HorizontalBar == null) return;

        _lastRegionRect = regionRect;
        _lastScreenSize = screenSize;

        VerticalBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        HorizontalBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var drawingSize = new Size(
            VerticalBar.ActualWidth > 0 ? VerticalBar.ActualWidth : VerticalBar.DesiredSize.Width,
            VerticalBar.ActualHeight > 0 ? VerticalBar.ActualHeight : VerticalBar.DesiredSize.Height);

        var actionSize = new Size(
            HorizontalBar.ActualWidth > 0 ? HorizontalBar.ActualWidth : HorizontalBar.DesiredSize.Width,
            HorizontalBar.ActualHeight > 0 ? HorizontalBar.ActualHeight : HorizontalBar.DesiredSize.Height);

        var posDrawing = ToolbarPositionHelper.CalculateDrawingBarPosition(regionRect, drawingSize, screenSize);
        var posAction = ToolbarPositionHelper.CalculateActionBarPosition(regionRect, actionSize, screenSize);

        Canvas.SetLeft(VerticalBar, posDrawing.X);
        Canvas.SetTop(VerticalBar, posDrawing.Y);

        Canvas.SetLeft(HorizontalBar, posAction.X);
        Canvas.SetTop(HorizontalBar, posAction.Y);

        UpdateFlyoutPosition();
    }

    /// <summary>
    /// Căn chỉnh vị trí của bảng Flyout (Phông chữ hoặc Cỡ số bước) bám sát cạnh thanh dọc
    /// và thẳng hàng với nút công cụ đang được kích hoạt, không để tràn màn hình.
    /// </summary>
    public void UpdateFlyoutPosition()
    {
        if (FlyoutOptionsPanel == null || FlyoutOptionsPanel.Visibility != Visibility.Visible) return;
        if (VerticalBar == null) return;

        var screenSize = _lastScreenSize.Width > 0
            ? _lastScreenSize
            : new Size(SystemParameters.PrimaryScreenWidth > 0 ? SystemParameters.PrimaryScreenWidth : 1920,
                       SystemParameters.PrimaryScreenHeight > 0 ? SystemParameters.PrimaryScreenHeight : 1080);

        FlyoutOptionsPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var flyoutWidth = FlyoutOptionsPanel.ActualWidth > 0 ? FlyoutOptionsPanel.ActualWidth : FlyoutOptionsPanel.DesiredSize.Width;
        var flyoutHeight = FlyoutOptionsPanel.ActualHeight > 0 ? FlyoutOptionsPanel.ActualHeight : FlyoutOptionsPanel.DesiredSize.Height;
        if (flyoutWidth <= 0) flyoutWidth = 190;
        if (flyoutHeight <= 0) flyoutHeight = 36;

        var vX = Canvas.GetLeft(VerticalBar);
        var vY = Canvas.GetTop(VerticalBar);
        if (double.IsNaN(vX) || double.IsNaN(vY))
        {
            var drawingSize = new Size(
                VerticalBar.ActualWidth > 0 ? VerticalBar.ActualWidth : 42,
                VerticalBar.ActualHeight > 0 ? VerticalBar.ActualHeight : 400);
            var pos = ToolbarPositionHelper.CalculateDrawingBarPosition(_lastRegionRect, drawingSize, screenSize);
            vX = pos.X;
            vY = pos.Y;
            Canvas.SetLeft(VerticalBar, vX);
            Canvas.SetTop(VerticalBar, vY);
        }

        var vWidth = VerticalBar.ActualWidth > 0 ? VerticalBar.ActualWidth : 42;

        // Horizontally: Ưu tiên đặt bên phải thanh dọc nếu còn đủ chỗ trên màn hình, ngược lại đặt bên trái
        double flyoutX;
        if (vX + vWidth + 6 + flyoutWidth <= screenSize.Width - 4)
        {
            flyoutX = vX + vWidth + 6;
        }
        else
        {
            flyoutX = vX - flyoutWidth - 6;
        }
        flyoutX = Math.Clamp(flyoutX, 4, Math.Max(4, screenSize.Width - flyoutWidth - 4));

        // Vertically: Canh theo nút công cụ đang được kích hoạt (TextButton hoặc StepCounterButton)
        FrameworkElement? targetBtn = _activeTool switch
        {
            DrawingToolType.Text => TextButton,
            DrawingToolType.StepCounter => StepCounterButton,
            _ => null
        };

        double btnOffsetY = 0;
        if (targetBtn != null)
        {
            try
            {
                var transform = targetBtn.TransformToAncestor(VerticalBar);
                var pt = transform.Transform(new Point(0, 0));
                btnOffsetY = pt.Y;
            }
            catch
            {
                btnOffsetY = _activeTool == DrawingToolType.Text ? 180 : 270;
            }
        }

        var flyoutY = vY + btnOffsetY;
        flyoutY = Math.Clamp(flyoutY, 4, Math.Max(4, screenSize.Height - flyoutHeight - 4));

        Canvas.SetLeft(FlyoutOptionsPanel, flyoutX);
        Canvas.SetTop(FlyoutOptionsPanel, flyoutY);
    }
}
