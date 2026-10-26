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
    private HotkeyGesture _pinClipboardHotkey;

    [ObservableProperty]
    private HotkeyGesture _repeatLastHotkey;

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

    /// <summary>
    /// Giao diện: "system" / "light" / "dark" (xem <see cref="AppTheme"/>). Cửa sổ Cài đặt đổi màu ngay khi chọn.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSystemTheme))]
    [NotifyPropertyChangedFor(nameof(IsLightTheme))]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    private string _theme = AppTheme.System;

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

    // Ba lựa chọn dạng RadioButton: chỉ đổi khi một nút được chọn (bỏ qua sự kiện bỏ chọn của nút cũ)
    public bool IsSystemTheme
    {
        get => Theme is not (AppTheme.Light or AppTheme.Dark);
        set { if (value) Theme = AppTheme.System; }
    }

    public bool IsLightTheme
    {
        get => Theme == AppTheme.Light;
        set { if (value) Theme = AppTheme.Light; }
    }

    public bool IsDarkTheme
    {
        get => Theme == AppTheme.Dark;
        set { if (value) Theme = AppTheme.Dark; }
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

    /// <summary>
    /// Các phím tắt có thể chỉnh trong cửa sổ Cài đặt, kèm cách đọc/ghi vào ViewModel và AppSettings.
    /// </summary>
    private IEnumerable<(HotkeyAction Action, Func<HotkeyGesture> Get, Action<HotkeyGesture> Set, Action<AppSettings, string> Store)> Hotkeys()
    {
        yield return (HotkeyAction.Capture, () => CaptureHotkey, g => CaptureHotkey = g, (s, v) => s.HotkeyCapture = v);
        yield return (HotkeyAction.CaptureAlternate, () => CaptureAlternateHotkey, g => CaptureAlternateHotkey = g, (s, v) => s.HotkeyCaptureAlternate = v);
        yield return (HotkeyAction.PinFromClipboard, () => PinClipboardHotkey, g => PinClipboardHotkey = g, (s, v) => s.HotkeyPinClipboard = v);
        yield return (HotkeyAction.RepeatLastCapture, () => RepeatLastHotkey, g => RepeatLastHotkey = g, (s, v) => s.HotkeyRepeatLast = v);
    }

    private void LoadFrom(AppSettings settings)
    {
        foreach (var hotkey in Hotkeys())
        {
            hotkey.Set(ActiveOrSaved(hotkey.Action, settings));
        }
        SaveDirectory = settings.DefaultSaveDirectory;
        AutoSaveToGallery = settings.AutoSaveToGallery;
        ImageFormat = ImageEncoderFactory.ExtensionFor(settings.ImageFormat) == ".jpg" ? "jpg" : "png";
        JpegQuality = Math.Clamp(settings.JpegQuality, 1, 100);
        LaunchAtStartup = SafeIsStartupEnabled();
        AutoCopyOnCapture = settings.AutoCopyOnCapture;
        Theme = settings.Theme is AppTheme.Light or AppTheme.Dark ? settings.Theme : AppTheme.System;
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
        foreach (var hotkey in Hotkeys())
        {
            HotkeyGesture.TryParse(AppHotkeyManager.DefaultGestureText(hotkey.Action), out var gesture);
            hotkey.Set(gesture);
        }
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
        var hotkeys = Hotkeys().ToList();
        var duplicate = hotkeys.GroupBy(h => h.Get()).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null)
        {
            ShowError($"Phím {duplicate.Key.DisplayText} đang được gán cho nhiều thao tác — mỗi thao tác cần một phím riêng.");
            return false;
        }

        var settings = _settingsService.CurrentSettings;
        foreach (var hotkey in hotkeys)
        {
            hotkey.Store(settings, hotkey.Get().ToString());
        }

        var failed = _hotkeyManager.Apply(settings);
        var failedNames = new List<string>();
        foreach (var hotkey in hotkeys.Where(h => failed.Contains(h.Action)))
        {
            failedNames.Add(hotkey.Get().DisplayText);

            // Giữ phím cũ đang hoạt động trong cả settings lẫn giao diện
            if (_hotkeyManager.GetActiveGesture(hotkey.Action) is { } active) hotkey.Set(active);
            hotkey.Store(settings, hotkey.Get().ToString());
        }

        settings.DefaultSaveDirectory = SaveDirectory;
        settings.AutoSaveToGallery = AutoSaveToGallery;
        settings.ImageFormat = ImageFormat;
        settings.JpegQuality = Math.Clamp(JpegQuality, 1, 100);
        settings.AutoCopyOnCapture = AutoCopyOnCapture;
        settings.Theme = Theme;
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
