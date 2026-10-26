using Microsoft.Win32;
using NeatShot.Common.Helpers;
using NeatShot.Common.Localization;
using NeatShot.Presentation.ViewModels;
using System.IO;
using System.Windows.Input;
using System.Windows;

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

        // Không bao giờ cao hơn vùng làm việc (trừ taskbar); phần thân tự cuộn khi thiếu chỗ
        MaxHeight = SystemParameters.WorkArea.Height;

        ApplyTheme();
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.Theme)) ApplyTheme();
        };

        // Đang gán phím mới: tạm nhả phím tắt toàn cục để Windows không "nuốt" tổ hợp đó
        foreach (var box in HotkeyBoxes)
        {
            box.RecordingStarted += (s, e) => ViewModel.BeginHotkeyRecording();
            box.RecordingEnded += (s, e) => ViewModel.EndHotkeyRecording();
        }

        Closed += (s, e) => ViewModel.EndHotkeyRecording();
        PreviewKeyDown += OnWindowPreviewKeyDown;
    }

    private const int ThemeDictionaryIndex = 1;

    /// <summary>
    /// Thay bảng màu (Window.Resources → MergedDictionaries[1]) theo lựa chọn Hệ thống / Sáng / Tối.
    /// Mọi màu trong cửa sổ dùng DynamicResource nên đổi ngay, không cần mở lại.
    /// </summary>
    private void ApplyTheme()
    {
        var light = AppTheme.IsLight(ViewModel.Theme, AppTheme.WindowsUsesLightTheme());
        var name = light ? "SettingsTheme.Light.xaml" : "SettingsTheme.Dark.xaml";
        Resources.MergedDictionaries[ThemeDictionaryIndex] = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/NeatShot;component/Presentation/Themes/{name}", UriKind.Absolute)
        };
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
            Title = Loc.T("Settings.ChooseFolderTitle"),
            InitialDirectory = Directory.Exists(ViewModel.SaveDirectory) ? ViewModel.SaveDirectory : string.Empty
        };

        if (dialog.ShowDialog(this) == true)
        {
            ViewModel.SaveDirectory = dialog.FolderName;
        }
    }
}
