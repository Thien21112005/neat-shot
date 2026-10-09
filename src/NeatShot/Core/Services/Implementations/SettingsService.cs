using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Dịch vụ lưu trữ và nạp cấu hình NeatShot vào thư mục %AppData%\NeatShot\settings.json.
/// Hỗ trợ xử lý lỗi file hỏng, khôi phục mặc định và đường dẫn tùy chỉnh khi kiểm thử.
/// </summary>
public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsFilePath;
    private readonly object _lock = new();

    public AppSettings CurrentSettings { get; private set; } = new();

    public SettingsService(string? settingsFilePath = null)
    {
        _settingsFilePath = string.IsNullOrWhiteSpace(settingsFilePath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NeatShot", "settings.json")
            : settingsFilePath;
    }

    public AppSettings LoadSettings()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                        if (loaded != null)
                        {
                            if (string.IsNullOrWhiteSpace(loaded.DefaultSaveDirectory))
                            {
                                loaded.DefaultSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "NeatShot");
                            }

                            CurrentSettings = loaded;
                            return CurrentSettings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Lỗi khi nạp file cấu hình: {ex.Message}");
            }

            CurrentSettings = new AppSettings();
            return CurrentSettings;
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_lock)
        {
            try
            {
                CurrentSettings = settings;
                var dir = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonSerializer.Serialize(CurrentSettings, JsonOptions);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Lỗi khi lưu file cấu hình: {ex.Message}");
            }
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            CurrentSettings = settings;
            var dir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(CurrentSettings, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Lỗi khi lưu async file cấu hình: {ex.Message}");
        }
    }
}
