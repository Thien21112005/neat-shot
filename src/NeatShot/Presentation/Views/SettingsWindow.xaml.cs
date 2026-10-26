using Microsoft.Win32;
using NeatShot.Presentation.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace NeatShot.Presentation.Views;

/// <summary>
/// Cửa sổ Cài đặt NeatShot (phím tắt, lưu ảnh, tuỳ chọn chung).
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        ViewModel.RequestClose += (s, e) => Close();

        // Đang gán phím mới: tạm nhả phím tắt toàn cục để Windows không "nuốt" tổ hợp đó
        foreach (var box in HotkeyBoxes)
        {
            box.RecordingStarted += (s, e) => ViewModel.BeginHotkeyRecording();
            box.RecordingEnded += (s, e) => ViewModel.EndHotkeyRecording();
        }

        Closed += (s, e) => ViewModel.EndHotkeyRecording();
        PreviewKeyDown += OnWindowPreviewKeyDown;
    }

    private IEnumerable<Controls.HotkeyBox> HotkeyBoxes =>
        new[] { CaptureHotkeyBox, CaptureAlternateHotkeyBox, PinClipboardHotkeyBox, RepeatLastHotkeyBox };

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var recording = HotkeyBoxes.Any(box => box.IsRecording);
        if (e.Key == Key.Escape && !recording)
        {
            Close();
            e.Handled = true;
        }
    }

    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Chuột đã nhả trước khi kéo
            }
        }
    }

    private void OnBrowseFolderClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Chọn thư mục lưu ảnh NeatShot",
            InitialDirectory = Directory.Exists(ViewModel.SaveDirectory) ? ViewModel.SaveDirectory : string.Empty
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.SaveDirectory = dialog.FolderName;
        }
    }
}
