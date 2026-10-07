using NeatShot.Core.Models;
using System.Windows;
using System.Windows.Media;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Tiện ích tính toán tỉ lệ thu phóng màn hình (Per-Monitor DPI v2)
/// và chuyển đổi toạ độ giữa WPF Device Independent Pixels (DIP) và Physical Device Pixels.
/// </summary>
public static class DpiHelper
{
    public static Point TransformToDevice(Point logicalPoint, double scaleX, double scaleY)
    {
        return new Point(logicalPoint.X * scaleX, logicalPoint.Y * scaleY);
    }

    public static Point TransformFromDevice(Point devicePoint, double scaleX, double scaleY)
    {
        if (scaleX <= 0) scaleX = 1.0;
        if (scaleY <= 0) scaleY = 1.0;
        return new Point(devicePoint.X / scaleX, devicePoint.Y / scaleY);
    }

    public static CaptureRegion TransformRegionToDevice(CaptureRegion logicalRegion, double scaleX, double scaleY)
    {
        return new CaptureRegion(
            logicalRegion.X * scaleX,
            logicalRegion.Y * scaleY,
            logicalRegion.Width * scaleX,
            logicalRegion.Height * scaleY);
    }

    public static CaptureRegion TransformRegionFromDevice(CaptureRegion deviceRegion, double scaleX, double scaleY)
    {
        if (scaleX <= 0) scaleX = 1.0;
        if (scaleY <= 0) scaleY = 1.0;

        return new CaptureRegion(
            deviceRegion.X / scaleX,
            deviceRegion.Y / scaleY,
            deviceRegion.Width / scaleX,
            deviceRegion.Height / scaleY);
    }

    /// <summary>
    /// Lấy DPI Scale hiện tại từ một Visual (Window/Control).
    /// </summary>
    public static DpiScale GetDpiScale(Visual visual)
    {
        return VisualTreeHelper.GetDpi(visual);
    }
}
