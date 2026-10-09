using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.IO;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Triển khai dịch vụ quản lý thư viện và lưu trữ ảnh chụp màn hình NeatShot.
/// </summary>
public class ScreenshotGalleryService : IScreenshotGalleryService
{
    private readonly string? _customDirectory;
    private readonly ISettingsService? _settingsService;

    public ScreenshotGalleryService(string? customDirectory = null, ISettingsService? settingsService = null)
    {
        _customDirectory = customDirectory;
        _settingsService = settingsService;
    }

    public ScreenshotGalleryService(ISettingsService settingsService) : this(null, settingsService)
    {
    }

    /// <inheritdoc />
    public string GetScreenshotsDirectory()
    {
        string dir;
        if (!string.IsNullOrWhiteSpace(_customDirectory))
        {
            dir = _customDirectory;
        }
        else if (_settingsService != null && !string.IsNullOrWhiteSpace(_settingsService.CurrentSettings.DefaultSaveDirectory))
        {
            dir = _settingsService.CurrentSettings.DefaultSaveDirectory;
        }
        else
        {
            dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "NeatShot");
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return dir;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GalleryItem>> GetSavedScreenshotsAsync()
    {
        var dir = GetScreenshotsDirectory();
        if (!Directory.Exists(dir))
        {
            return Array.Empty<GalleryItem>();
        }

        return await Task.Run(() =>
        {
            var directoryInfo = new DirectoryInfo(dir);
            var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".png", ".jpg", ".jpeg", ".bmp"
            };

            var files = directoryInfo.EnumerateFiles()
                .Where(f => supportedExtensions.Contains(f.Extension))
                .ToList();

            var items = new List<GalleryItem>();

            foreach (var file in files)
            {
                var item = new GalleryItem
                {
                    FilePath = file.FullName,
                    FileName = file.Name,
                    CreatedAt = file.LastWriteTime > file.CreationTime ? file.LastWriteTime : file.CreationTime,
                    FileSizeBytes = file.Length
                };

                try
                {
                    using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    if (decoder.Frames.Count > 0)
                    {
                        item.PixelWidth = decoder.Frames[0].PixelWidth;
                        item.PixelHeight = decoder.Frames[0].PixelHeight;
                    }
                }
                catch
                {
                    // Nếu lỗi giải mã header thì bỏ qua chiều rộng/cao
                }

                items.Add(item);
            }

            return (IReadOnlyList<GalleryItem>)items.OrderByDescending(i => i.CreatedAt).ToList();
        });
    }

    /// <inheritdoc />
    public async Task<string> SaveScreenshotAsync(BitmapSource image, string? filename = null)
    {
        ArgumentNullException.ThrowIfNull(image);

        var dir = GetScreenshotsDirectory();

        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = $"NeatShot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        }

        var fullPath = Path.Combine(dir, filename);
        if (File.Exists(fullPath))
        {
            var baseName = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);
            fullPath = Path.Combine(dir, $"{baseName}_{DateTime.Now:fff}{ext}");
        }

        await Task.Run(() =>
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(stream);
        });

        return fullPath;
    }

    /// <inheritdoc />
    public bool DeleteScreenshot(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScreenshotGalleryService] Không thể xoá file: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public BitmapSource? LoadImage(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            var ms = new MemoryStream(bytes);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScreenshotGalleryService] Không thể nạp ảnh {filePath}: {ex.Message}");
            return null;
        }
    }
}
