using System.Windows;
using System.Windows.Media;

namespace NeatShot.Core.Models;

/// <summary>
/// Các kiểu phối màu nền gradient làm đẹp có sẵn.
/// </summary>
public enum BeautifyPreset
{
    None,
    Sunset,
    Ocean,
    Purple,
    DarkSlate
}

/// <summary>
/// Các tuỳ chọn làm đẹp ảnh chụp màn hình (khoảng đệm, bo góc, đổ bóng mềm và nền gradient).
/// </summary>
public class BeautifyOptions
{
    public bool IsEnabled { get; set; } = true;
    public double Padding { get; set; } = 20.0;
    public double CornerRadius { get; set; } = 12.0;
    public double ShadowBlurRadius { get; set; } = 20.0;
    public double ShadowOpacity { get; set; } = 0.35;
    public BeautifyPreset Preset { get; set; } = BeautifyPreset.Sunset;

    /// <summary>
    /// Tạo cọ vẽ nền tương ứng với Preset đang chọn.
    /// </summary>
    public Brush CreateBackgroundBrush()
    {
        return Preset switch
        {
            BeautifyPreset.Sunset => new LinearGradientBrush(
                Color.FromRgb(0xFA, 0x70, 0x9A),
                Color.FromRgb(0xFE, 0xE1, 0x40),
                new Point(0, 0),
                new Point(1, 1)),

            BeautifyPreset.Ocean => new LinearGradientBrush(
                Color.FromRgb(0x00, 0x93, 0xE9),
                Color.FromRgb(0x80, 0xD0, 0xC7),
                new Point(0, 0),
                new Point(1, 1)),

            BeautifyPreset.Purple => new LinearGradientBrush(
                Color.FromRgb(0x66, 0x7E, 0xEA),
                Color.FromRgb(0x76, 0x4B, 0xA2),
                new Point(0, 0),
                new Point(1, 1)),

            BeautifyPreset.DarkSlate => new LinearGradientBrush(
                Color.FromRgb(0x1E, 0x29, 0x3B),
                Color.FromRgb(0x0F, 0x17, 0x2A),
                new Point(0, 0),
                new Point(1, 1)),

            _ => Brushes.Transparent
        };
    }
}
