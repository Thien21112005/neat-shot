using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NeatShot.Presentation.Converters;

/// <summary>
/// Chuyển đổi đường dẫn file ảnh thành BitmapSource nạp vào bộ nhớ (OnLoad)
/// để không khóa file trên ổ đĩa, tối ưu RAM cho thumbnail.
/// </summary>
public class FilePathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && File.Exists(path))
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                using var ms = new MemoryStream(bytes);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = 320;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
