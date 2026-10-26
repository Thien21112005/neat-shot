using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeatShot.Common.Helpers;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;

namespace NeatShot.Presentation.ViewModels;

/// <summary>
/// ViewModel của cửa sổ Cài đặt: phím tắt, lưu ảnh và tuỳ chọn chung.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IStartupService _startupService;
    private readonly IAppHotkeyManager _hotkeyManager;
    private readonly Func<string?> _exePathProvider;

    [ObservableProperty]
    private HotkeyGesture _captureHotkey;

    [ObservableProperty]
    private HotkeyGesture _captureAlternateHotkey;

    [ObservableProperty]
    private string _saveDirectory = string.Empty;

    [ObservableProperty]
    private bool _autoSaveToGallery;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJpg))]
    [NotifyPropertyChangedFor(nameof(IsPng))]
    private string _imageFormat = "png";

    [ObservableProperty]
    private int _jpegQuality;

    [ObservableProperty]
    private bool _launchAtStartup;

    [ObservableProperty]
    private bool _autoCopyOnCapture;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _hasError;

    public event EventHandler? RequestClose;

    public bool IsJpg
    {
        get => ImageFormat == "jpg";
        set => ImageFormat = value ? "jpg" : "png";
    }

    public bool IsPng
    {
        get => ImageFormat != "jpg";
        set => ImageFormat = value ? "png" : "jpg";
    }

    public SettingsViewModel(
        ISettingsService settingsService,
        IStartupService startupService,
        IAppHotkeyManager hotkeyManager,
        Func<string?>? exePathProvider = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
        _hotkeyManager = hotkeyManager ?? throw new ArgumentNullException(nameof(hotkeyManager));
        _exePathProvider = exePathProvider ?? (() => Environment.ProcessPath);

        LoadFrom(_settingsService.CurrentSettings);
    }

    private void LoadFrom(AppSettings settings)
    {
        CaptureHotkey = ActiveOrSaved(HotkeyAction.Capture, settings);
        CaptureAlternateHotkey = ActiveOrSaved(HotkeyAction.CaptureAlternate, settings);
        SaveDirectory = settings.DefaultSaveDirectory;
        AutoSaveToGallery = settings.AutoSaveToGallery;
        ImageFormat = ImageEncoderFactory.ExtensionFor(settings.ImageFormat) == ".jpg" ? "jpg" : "png";
        JpegQuality = Math.Clamp(settings.JpegQuality, 1, 100);
        LaunchAtStartup = SafeIsStartupEnabled();
        AutoCopyOnCapture = settings.AutoCopyOnCapture;
    }

    private HotkeyGesture ActiveOrSaved(HotkeyAction action, AppSettings settings)
    {
        if (_hotkeyManager.GetActiveGesture(action) is { } active) return active;
        if (HotkeyGesture.TryParse(AppHotkeyManager.GestureText(settings, action), out var saved)) return saved;
        HotkeyGesture.TryParse(AppHotkeyManager.DefaultGestureText(action), out var fallback);
        return fallback;
    }

    private bool SafeIsStartupEnabled()
    {
        try
        {
            return _startupService.IsEnabled();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Người dùng bắt đầu bấm tổ hợp phím mới: tạm nhả phím tắt toàn cục để Windows không "nuốt" tổ hợp đó.
    /// </summary>
    public void BeginHotkeyRecording() => _hotkeyManager.Suspend();

    /// <summary>
    /// Kết thúc ghi phím tắt: đăng ký lại các phím tắt hiện hành.
    /// </summary>
    public void EndHotkeyRecording() => _hotkeyManager.Resume();

    [RelayCommand]
    private void ResetHotkeys()
    {
        HotkeyGesture.TryParse(AppHotkeyManager.DefaultGestureText(HotkeyAction.Capture), out var capture);
        HotkeyGesture.TryParse(AppHotkeyManager.DefaultGestureText(HotkeyAction.CaptureAlternate), out var alternate);
        CaptureHotkey = capture;
        CaptureAlternateHotkey = alternate;
    }

    [RelayCommand]
    private void SaveAndClose()
    {
        if (Save())
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Lưu toàn bộ cài đặt và áp dụng ngay (phím tắt, khởi động cùng Windows).
    /// </summary>
    /// <returns>false nếu có phím tắt không áp dụng được (phím cũ được giữ lại).</returns>
    public bool Save()
    {
        if (CaptureHotkey == CaptureAlternateHotkey)
        {
            ShowError("Phím chụp chính và phím chụp phụ đang trùng nhau.");
            return false;
        }

        var settings = _settingsService.CurrentSettings;
        settings.HotkeyCapture = CaptureHotkey.ToString();
        settings.HotkeyCaptureAlternate = CaptureAlternateHotkey.ToString();

        var failed = _hotkeyManager.Apply(settings);
        var failedNames = new List<string>();
        foreach (var action in failed)
        {
            var attempted = action == HotkeyAction.Capture ? CaptureHotkey : CaptureAlternateHotkey;
            failedNames.Add(attempted.DisplayText);

            // Giữ phím cũ đang hoạt động trong cả settings lẫn giao diện
            var active = _hotkeyManager.GetActiveGesture(action);
            if (action == HotkeyAction.Capture)
            {
                if (active is { } a) CaptureHotkey = a;
                settings.HotkeyCapture = CaptureHotkey.ToString();
            }
            else
            {
                if (active is { } a) CaptureAlternateHotkey = a;
                settings.HotkeyCaptureAlternate = CaptureAlternateHotkey.ToString();
            }
        }

        settings.DefaultSaveDirectory = SaveDirectory;
        settings.AutoSaveToGallery = AutoSaveToGallery;
        settings.ImageFormat = ImageFormat;
        settings.JpegQuality = Math.Clamp(JpegQuality, 1, 100);
        settings.AutoCopyOnCapture = AutoCopyOnCapture;
        settings.LaunchAtStartup = LaunchAtStartup;
        ApplyStartup();

        _settingsService.SaveSettings(settings);

        if (failedNames.Count > 0)
        {
            ShowError($"Phím {string.Join(", ", failedNames)} đang được ứng dụng khác sử dụng — đã giữ phím cũ.");
            return false;
        }

        HasError = false;
        StatusMessage = "Đã lưu cài đặt.";
        return true;
    }

    private void ApplyStartup()
    {
        try
        {
            var exePath = _exePathProvider();
            if (LaunchAtStartup && !string.IsNullOrEmpty(exePath))
            {
                _startupService.Enable(exePath);
            }
            else if (!LaunchAtStartup)
            {
                _startupService.Disable();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Không thể cập nhật khởi động cùng Windows: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        HasError = true;
        StatusMessage = message;
    }
}
