using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeatShot.Presentation.Controls;

public partial class AnnotationToolbar : UserControl
{
    public event EventHandler<DrawingToolType>? ToolSelected;
    public event EventHandler<Color>? ColorSelected;
    public event EventHandler? UndoRequested;
    public event EventHandler? CopyRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? CloseRequested;

    public AnnotationToolbar()
    {
        InitializeComponent();
    }

    private void OnPencilClick(object sender, RoutedEventArgs e)
    {
        ToolSelected?.Invoke(this, DrawingToolType.Pencil);
    }

    private void OnRectClick(object sender, RoutedEventArgs e)
    {
        ToolSelected?.Invoke(this, DrawingToolType.Rectangle);
    }

    private void OnArrowClick(object sender, RoutedEventArgs e)
    {
        ToolSelected?.Invoke(this, DrawingToolType.Arrow);
    }

    private void OnColorSelect(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
        {
            var color = ColorHelper.FromHex(hex);
            ColorSelected?.Invoke(this, color);
        }
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
