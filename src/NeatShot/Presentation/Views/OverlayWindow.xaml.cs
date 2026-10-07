using NeatShot.Core.Models;
using NeatShot.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.Views;

public partial class OverlayWindow : Window
{
    public OverlayViewModel ViewModel { get; }

    public OverlayWindow(OverlayViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.RequestClose += (s, e) => Close();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        InitializeToolbar();
    }

    private void InitializeToolbar()
    {
        Toolbar.ToolSelected += (s, tool) => DrawingControl.CurrentTool = tool;
        Toolbar.ColorSelected += (s, color) => DrawingControl.CurrentColor = color;
        Toolbar.UndoRequested += (s, e) => DrawingControl.Undo();
        Toolbar.CloseRequested += (s, e) => ViewModel.CancelCommand.Execute(null);

        // Copy và Save sẽ được xử lý trong Task 7
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayViewModel.SelectedRegion))
        {
            UpdateToolbarPosition(ViewModel.SelectedRegion);
        }
    }

    private void UpdateToolbarPosition(CaptureRegion region)
    {
        if (region.IsValid)
        {
            var rect = region.ToRect();
            Toolbar.Visibility = Visibility.Visible;

            // Đặt Toolbar nằm ngay phía dưới vùng chọn
            var toolbarLeft = rect.Right - 420; // căn phải theo vùng chọn
            if (toolbarLeft < rect.Left) toolbarLeft = rect.Left;
            if (toolbarLeft < 10) toolbarLeft = 10;

            var toolbarTop = rect.Bottom + 10;
            // Nếu sát đáy màn hình thì đưa Toolbar lên phía trên vùng chọn
            if (toolbarTop + 50 > ActualHeight)
            {
                toolbarTop = rect.Top - 45;
                if (toolbarTop < 10) toolbarTop = 10;
            }

            Canvas.SetLeft(Toolbar, toolbarLeft);
            Canvas.SetTop(Toolbar, toolbarTop);
        }
        else
        {
            Toolbar.Visibility = Visibility.Collapsed;
        }
    }

    public void Display(Rect virtualBounds, BitmapSource frozenScreen)
    {
        Left = virtualBounds.Left;
        Top = virtualBounds.Top;
        Width = virtualBounds.Width;
        Height = virtualBounds.Height;

        ViewModel.BackgroundImage = frozenScreen;

        Show();
        Activate();
        Focus();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ViewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            DrawingControl.Undo();
            e.Handled = true;
        }
    }
}
