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

    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler? UndoRequested;
    public event EventHandler? CopyRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CloseRequested;

    public AnnotationToolbar()
    {
        InitializeComponent();
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
    }

    public void ResetTools()
    {
        _activeTool = DrawingToolType.None;
        UpdateToolButtonVisuals();
        UpdateColorButtonVisuals();
        ToolSelected?.Invoke(this, DrawingToolType.None);
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
        SetButtonActive(PencilButton, _activeTool == DrawingToolType.Pencil);
        SetButtonActive(RectButton, _activeTool == DrawingToolType.Rectangle);
        SetButtonActive(ArrowButton, _activeTool == DrawingToolType.Arrow);
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

    private void OnPencilClick(object sender, RoutedEventArgs e)
    {
        ToggleTool(DrawingToolType.Pencil);
    }

    private void OnRectClick(object sender, RoutedEventArgs e)
    {
        ToggleTool(DrawingToolType.Rectangle);
    }

    private void OnArrowClick(object sender, RoutedEventArgs e)
    {
        ToggleTool(DrawingToolType.Arrow);
    }

    private void OnColorSelect(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
        {
            _selectedColorHex = hex;

            // Nếu người dùng chọn màu khi chưa chọn công cụ, tự động bật công cụ Bút (Pencil)
            if (_activeTool == DrawingToolType.None)
            {
                _activeTool = DrawingToolType.Pencil;
                UpdateToolButtonVisuals();
                ToolSelected?.Invoke(this, _activeTool);
            }

            UpdateColorButtonVisuals();
            var color = ColorHelper.FromHex(hex);
            ColorSelected?.Invoke(this, color);
        }
    }

    private void UpdateColorButtonVisuals()
    {
        UpdateColorBorder(ColorRedButton);
        UpdateColorBorder(ColorYellowButton);
        UpdateColorBorder(ColorGreenButton);
        UpdateColorBorder(ColorBlueButton);
        UpdateColorBorder(ColorWhiteButton);
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

    private void OnUndoClick(object sender, RoutedEventArgs e)
    {
        UndoRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        CopyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
