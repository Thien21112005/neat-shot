namespace NeatShot.Core.Models;

/// <summary>
/// Đại diện cho một file ảnh chụp màn hình trong thư viện NeatShot.
/// </summary>
public class GalleryItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public int PixelWidth { get; set; }
    public int PixelHeight { get; set; }

    public string FormattedSize => FileSizeBytes switch
    {
        >= 1024 * 1024 => $"{(double)FileSizeBytes / (1024 * 1024):0.1} MB",
        >= 1024 => $"{FileSizeBytes / 1024} KB",
        _ => $"{FileSizeBytes} B"
    };

    public string FormattedDimensions => $"{PixelWidth} × {PixelHeight}";
}
