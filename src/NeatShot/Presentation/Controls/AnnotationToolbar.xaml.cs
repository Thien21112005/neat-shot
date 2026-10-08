using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
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

    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler<string>? FontFamilyChanged;
    public event EventHandler<double>? FontSizeChanged;
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

    private bool _isDraggingToolbar;
    private Point _dragStartPoint;
    private bool _isUpdatingSelection;
    private bool _isInitialized;

    public AnnotationToolbar()
    {
        InitializeComponent();
        _isInitialized = true;
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
            _isDraggingToolbar = true;
            _dragStartPoint = e.GetPosition(this);
            DragGrip.CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnDragGripMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingToolbar)
        {
            var cur = e.GetPosition(this);
            var delta = new Point(cur.X - _dragStartPoint.X, cur.Y - _dragStartPoint.Y);
            ToolbarMoved?.Invoke(this, delta);
            e.Handled = true;
        }
    }

    private void OnDragGripMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDraggingToolbar)
        {
            _isDraggingToolbar = false;
            DragGrip.ReleaseMouseCapture();
            e.Handled = true;
        }
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
        SelectedFontSize = fontSize;
        if (FontSizeComboBox != null)
        {
            foreach (ComboBoxItem item in FontSizeComboBox.Items)
            {
                if (double.TryParse(item.Content?.ToString(), out var sz) && Math.Abs(sz - fontSize) < 0.1)
                {
                    FontSizeComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        FontSizeChanged?.Invoke(this, fontSize);
    }

    /// <summary>
    /// Chọn màu hiện tại và kích hoạt sự kiện ColorSelected.
    /// </summary>
    public void SelectColor(Color color)
    {
        _selectedColor = color;
        _selectedColorHex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

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

        if (FontControlsPanel != null)
        {
            FontControlsPanel.Visibility = _activeTool == DrawingToolType.Text ? Visibility.Visible : Visibility.Collapsed;
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

    private void OnShapeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (ShapeComboBox?.SelectedItem is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Ellipse" ? DrawingToolType.Ellipse : DrawingToolType.Rectangle;
            SelectShapeTool(tool);
        }
    }

    private void OnLineComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized || _isUpdatingSelection) return;
        if (LineComboBox?.SelectedItem is ComboBoxItem item)
        {
            var tool = item.Tag?.ToString() == "Line" ? DrawingToolType.Line : DrawingToolType.Arrow;
            SelectLineTool(tool);
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
            SelectedFontSize = size;
            FontSizeChanged?.Invoke(this, SelectedFontSize);
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

    private void UpdateColorButtonVisuals()
    {
        if (!_isInitialized || ColorRedButton == null) return;

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

        // CHỈ hiển thị viền chọn màu khi đang có công cụ vẽ được kích hoạt (_activeTool != None)
        var isSelected = _activeTool != DrawingToolType.None &&
                         btn.Tag is string hex &&
                         string.Equals(hex, _selectedColorHex, StringComparison.OrdinalIgnoreCase);

        btn.BorderBrush = isSelected ? Brushes.White : Brushes.Transparent;
        btn.BorderThickness = isSelected ? new Thickness(2) : new Thickness(1);
    }

    private void UpdateHistorySwatches()
    {
        HistoryColorsPanel.Children.Clear();
        foreach (var col in History.Colors)
        {
            var isSelected = _activeTool != DrawingToolType.None &&
                             col.A == _selectedColor.A &&
                             col.R == _selectedColor.R &&
                             col.G == _selectedColor.G &&
                             col.B == _selectedColor.B;

            var btn = new Button
            {
                Margin = new Thickness(2, 0, 2, 0),
                Padding = new Thickness(2),
                Tag = col,
                ToolTip = $"#{col.R:X2}{col.G:X2}{col.B:X2}",
                Background = Brushes.Transparent,
                BorderBrush = isSelected ? Brushes.White : Brushes.Transparent,
                BorderThickness = isSelected ? new Thickness(2) : new Thickness(1),
                Content = new Border
                {
                    Width = 14,
                    Height = 14,
                    CornerRadius = new CornerRadius(7),
                    Background = new SolidColorBrush(col)
                }
            };
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
                    Padding = 32.0,
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
}
