using NeatShot.Core.Models;
using NeatShot.Core.Services.Implementations;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NeatShot.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _settingsFilePath;

    public SettingsServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "NeatShot_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _settingsFilePath = Path.Combine(_tempDirectory, "settings.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    [Fact]
    public void LoadSettings_ReturnsDefaultSettings_WhenFileDoesNotExist()
    {
        // Arrange
        ISettingsService service = new SettingsService(_settingsFilePath);

        // Act
        var settings = service.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.RecentColorsHex);
        Assert.Empty(settings.RecentColorsHex);
        Assert.Equal("Ctrl+Shift+A", settings.HotkeyCapture);
        Assert.Same(settings, service.CurrentSettings);
    }

    [Fact]
    public async Task SaveSettingsAsync_And_LoadSettings_PersistsDataCorrectly()
    {
        // Arrange
        ISettingsService service = new SettingsService(_settingsFilePath);
        var expectedSettings = new AppSettings
        {
            RecentColorsHex = new List<string> { "#FFFF0000", "#FF00FF00", "#FF0000FF" },
            DefaultSaveDirectory = @"C:\Users\Test\Pictures",
            HotkeyCapture = "Ctrl+Shift+S",
            AutoCopyOnCapture = true
        };

        // Act
        await service.SaveSettingsAsync(expectedSettings);

        // Create a new instance to ensure it reads from disk
        ISettingsService newService = new SettingsService(_settingsFilePath);
        var loadedSettings = newService.LoadSettings();

        // Assert
        Assert.NotNull(loadedSettings);
        Assert.Equal(expectedSettings.RecentColorsHex, loadedSettings.RecentColorsHex);
        Assert.Equal(expectedSettings.DefaultSaveDirectory, loadedSettings.DefaultSaveDirectory);
        Assert.Equal(expectedSettings.HotkeyCapture, loadedSettings.HotkeyCapture);
        Assert.Equal(expectedSettings.AutoCopyOnCapture, loadedSettings.AutoCopyOnCapture);
    }

    [Fact]
    public void SaveSettings_Synchronous_PersistsDataCorrectly()
    {
        // Arrange
        ISettingsService service = new SettingsService(_settingsFilePath);
        var expectedSettings = new AppSettings
        {
            RecentColorsHex = new List<string> { "#FFFF0000" },
            DefaultSaveDirectory = @"D:\Captures",
            HotkeyCapture = "PrintScreen",
            AutoCopyOnCapture = false
        };

        // Act
        service.SaveSettings(expectedSettings);

        // Assert
        Assert.True(File.Exists(_settingsFilePath));
        ISettingsService newService = new SettingsService(_settingsFilePath);
        var loaded = newService.LoadSettings();
        Assert.Equal(expectedSettings.DefaultSaveDirectory, loaded.DefaultSaveDirectory);
        Assert.Equal(expectedSettings.RecentColorsHex, loaded.RecentColorsHex);
    }

    [Fact]
    public void LoadSettings_ReturnsDefaultSettings_WhenJsonIsCorrupt()
    {
        // Arrange
        File.WriteAllText(_settingsFilePath, "{ invalid json text : [[[[");
        ISettingsService service = new SettingsService(_settingsFilePath);

        // Act
        var settings = service.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.RecentColorsHex);
        Assert.Empty(settings.RecentColorsHex);
        Assert.Equal("Ctrl+Shift+A", settings.HotkeyCapture);
    }

    [Fact]
    public void LoadSettings_ReturnsDefaultSettings_WhenFileIsEmpty()
    {
        // Arrange
        File.WriteAllText(_settingsFilePath, "");
        ISettingsService service = new SettingsService(_settingsFilePath);

        // Act
        var settings = service.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.RecentColorsHex);
        Assert.Empty(settings.RecentColorsHex);
        Assert.Equal("Ctrl+Shift+A", settings.HotkeyCapture);
    }
}
