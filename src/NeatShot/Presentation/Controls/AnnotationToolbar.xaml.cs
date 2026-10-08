using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
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

    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? CopyRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CloseRequested;

    public AnnotationToolbar()
    {
        InitializeComponent();
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
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
    /// Kích hoạt một công cụ cụ thể từ bên ngoài (ví dụ sau khi kết thúc Eyedropper).
    /// </summary>
    public void SetActiveTool(DrawingToolType tool)
    {
        _activeTool = tool;
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
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
        SetButtonActive(EyedropperButton, _activeTool == DrawingToolType.Eyedropper);
    }

    private static void SetButtonActive(Button btn, bool isActive)
    {
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

    private void OnSelectClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Select);
    private void OnPencilClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Pencil);
    private void OnRectClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Rectangle);
    private void OnEllipseClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Ellipse);
    private void OnLineClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Line);
    private void OnArrowClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Arrow);
    private void OnHighlightClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Highlight);
    private void OnTextClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Text);
    private void OnEyedropperClick(object sender, RoutedEventArgs e) => ToggleTool(DrawingToolType.Eyedropper);

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
        UpdateColorBorder(ColorRedButton);
        UpdateColorBorder(ColorYellowButton);
        UpdateColorBorder(ColorGreenButton);
        UpdateColorBorder(ColorBlueButton);
        UpdateColorBorder(ColorWhiteButton);
        UpdateHistorySwatches();
    }

    private void UpdateColorBorder(Button btn)
    {
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
    private void OnCloseClick(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}
