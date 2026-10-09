using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

public partial class PinAnnotationToolbar : UserControl
{
    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler? UndoRequested;
    public event EventHandler? CloseRequested;

    public DrawingToolType CurrentTool { get; private set; } = DrawingToolType.None;
    public Color CurrentColor { get; private set; } = Colors.Red;

    public PinAnnotationToolbar()
    {
        InitializeComponent();
    }

    public void SelectTool(DrawingToolType tool)
    {
        CurrentTool = tool;
        UpdateActiveButtonVisual();
        ToolSelected?.Invoke(this, tool);
    }

    public void SelectColor(Color color)
    {
        CurrentColor = color;
        ColorSelected?.Invoke(this, color);
    }

    private void UpdateActiveButtonVisual()
    {
        ResetButtonHighlight(SelectButton);
        ResetButtonHighlight(PencilButton);
        ResetButtonHighlight(RectButton);
        ResetButtonHighlight(ArrowButton);
        ResetButtonHighlight(TextButton);
        ResetButtonHighlight(StepButton);

        var activeBtn = CurrentTool switch
        {
            DrawingToolType.Select => SelectButton,
            DrawingToolType.Pencil => PencilButton,
            DrawingToolType.Rectangle => RectButton,
            DrawingToolType.Arrow => ArrowButton,
            DrawingToolType.Text => TextButton,
            DrawingToolType.StepCounter => StepButton,
            _ => null
        };

        if (activeBtn != null)
        {
            activeBtn.Background = new SolidColorBrush(Color.FromArgb(90, 0, 120, 212));
            activeBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 120, 212));
        }
    }

    private static void ResetButtonHighlight(Button btn)
    {
        btn.Background = Brushes.Transparent;
        btn.BorderBrush = Brushes.Transparent;
    }

    private void OnSelectClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Select);
    private void OnPencilClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Pencil);
    private void OnRectClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Rectangle);
    private void OnArrowClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Arrow);
    private void OnTextClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.Text);
    private void OnStepClick(object sender, RoutedEventArgs e) => SelectTool(DrawingToolType.StepCounter);

    private void OnColorRedClick(object sender, RoutedEventArgs e) => SelectColor(Color.FromRgb(255, 59, 48));
    private void OnColorYellowClick(object sender, RoutedEventArgs e) => SelectColor(Color.FromRgb(255, 204, 0));
    private void OnColorGreenClick(object sender, RoutedEventArgs e) => SelectColor(Color.FromRgb(52, 199, 89));
    private void OnColorBlueClick(object sender, RoutedEventArgs e) => SelectColor(Color.FromRgb(0, 120, 212));
    private void OnColorWhiteClick(object sender, RoutedEventArgs e) => SelectColor(Colors.White);

    private void OnUndoClick(object sender, RoutedEventArgs e) => UndoRequested?.Invoke(this, EventArgs.Empty);
    private void OnHideClick(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}
