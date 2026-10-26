using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

/// <summary>
/// Thanh vẽ chú thích trên ảnh ghim — cùng bộ icon và đủ công cụ như thanh chú thích khi chụp.
/// </summary>
public partial class PinAnnotationToolbar : UserControl
{
    private static readonly Brush ActiveBackground = Frozen(Color.FromRgb(0, 120, 212));

    /// <summary>
    /// Đánh dấu ô màu đang được dùng (vẽ vòng sáng quanh ô).
    /// </summary>
    public static readonly DependencyProperty IsCurrentProperty =
        DependencyProperty.RegisterAttached("IsCurrent", typeof(bool), typeof(PinAnnotationToolbar), new PropertyMetadata(false));

    public static bool GetIsCurrent(DependencyObject element) => (bool)element.GetValue(IsCurrentProperty);
    public static void SetIsCurrent(DependencyObject element, bool value) => element.SetValue(IsCurrentProperty, value);

    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? CloseRequested;

    public DrawingToolType CurrentTool { get; private set; } = DrawingToolType.None;
    public Color CurrentColor { get; private set; } = Color.FromRgb(255, 59, 48);

    public PinAnnotationToolbar()
    {
        InitializeComponent();
        SetIsCurrent(ColorRedButton, true);
    }

    private IEnumerable<(Button Button, DrawingToolType Tool)> ToolButtons =>
    [
        (SelectButton, DrawingToolType.Select),
        (PencilButton, DrawingToolType.Pencil),
        (HighlightButton, DrawingToolType.Highlight),
        (RectButton, DrawingToolType.Rectangle),
        (EllipseButton, DrawingToolType.Ellipse),
        (LineButton, DrawingToolType.Line),
        (ArrowButton, DrawingToolType.Arrow),
        (TextButton, DrawingToolType.Text),
        (StepButton, DrawingToolType.StepCounter),
        (PixelateButton, DrawingToolType.Pixelate),
        (BlurButton, DrawingToolType.Blur),
    ];

    private IEnumerable<Button> Swatches =>
        [ColorRedButton, ColorYellowButton, ColorGreenButton, ColorBlueButton, ColorWhiteButton, ColorBlackButton];

    public void SelectTool(DrawingToolType tool)
    {
        CurrentTool = tool;

        // Công cụ đang chọn: nền xanh đặc + icon trắng, giống thanh chú thích chính
        foreach (var (button, buttonTool) in ToolButtons)
        {
            if (buttonTool == tool)
            {
                button.Background = ActiveBackground;
                button.Foreground = Brushes.White;
            }
            else
            {
                button.ClearValue(BackgroundProperty);
                button.ClearValue(ForegroundProperty);
            }
        }

        ToolSelected?.Invoke(this, tool);
    }

    public void SelectColor(Color color)
    {
        CurrentColor = color;
        foreach (var swatch in Swatches)
        {
            SetIsCurrent(swatch, swatch.Background is SolidColorBrush brush && brush.Color == color);
        }
        ColorSelected?.Invoke(this, color);
    }

    private void OnSelectClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Select);
    private void OnPencilClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Pencil);
    private void OnHighlightClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Highlight);
    private void OnRectClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Rectangle);
    private void OnEllipseClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Ellipse);
    private void OnLineClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Line);
    private void OnArrowClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Arrow);
    private void OnTextClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Text);
    private void OnStepClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.StepCounter);
    private void OnPixelateClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Pixelate);
    private void OnBlurClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Blur);

    private void OnColorClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Background: SolidColorBrush brush })
        {
            SelectColor(brush.Color);
        }
    }

    private void OnUndoClick(object sender, RoutedEventArgs e) => UndoRequested?.Invoke(this, EventArgs.Empty);
    private void OnRedoClick(object sender, RoutedEventArgs e) => RedoRequested?.Invoke(this, EventArgs.Empty);
    private void OnHideClick(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private static Brush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
