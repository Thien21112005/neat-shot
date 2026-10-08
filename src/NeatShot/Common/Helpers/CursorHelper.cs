using NeatShot.Core.Interop;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Windows.Interop;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Cung cấp con trỏ chuột tùy biến (Custom Cursor) chất lượng cao cho NeatShot,
/// đặc biệt là con trỏ chữ thập màu trắng sáng viền tương phản cao để ngắm cắt vùng chính xác.
/// </summary>
public static class CursorHelper
{
    private static Cursor? _whiteCrosshair;

    /// <summary>
    /// Con trỏ chữ thập màu trắng sáng (Bright White Crosshair) với viền bóng đen mờ tương phản,
    /// hiển thị sắc nét trên cả nền ảnh tối và nền ảnh sáng.
    /// </summary>
    public static Cursor WhiteCrosshair => _whiteCrosshair ??= CreateWhiteCrosshairCursor();

    public static Cursor CreateWhiteCrosshairCursor()
    {
        try
        {
            const int size = 32;
            const int center = 15;
            const int armLength = 11;
            const int gap = 2; // Khoảng hở 2px ở tâm để nhìn rõ điểm ảnh mục tiêu

            using var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.Clear(Color.Transparent);

                // 1. Vẽ lớp bóng đen tương phản (Black outline/shadow) để nổi bật trên nền sáng
                using var shadowPen = new Pen(Color.FromArgb(180, 0, 0, 0), 3f);
                // Nhánh trái
                g.DrawLine(shadowPen, center - armLength, center, center - gap, center);
                // Nhánh phải
                g.DrawLine(shadowPen, center + gap, center, center + armLength, center);
                // Nhánh trên
                g.DrawLine(shadowPen, center, center - armLength, center, center - gap);
                // Nhánh dưới
                g.DrawLine(shadowPen, center, center + gap, center, center + armLength);

                // Điểm tâm đen nhẹ
                using var dotShadow = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                g.FillRectangle(dotShadow, center - 1, center - 1, 3, 3);

                // 2. Vẽ lõi chữ thập màu trắng tinh sáng rực rỡ (Bright White core)
                using var whitePen = new Pen(Color.White, 1.2f);
                // Nhánh trái
                g.DrawLine(whitePen, center - armLength, center, center - gap, center);
                // Nhánh phải
                g.DrawLine(whitePen, center + gap, center, center + armLength, center);
                // Nhánh trên
                g.DrawLine(whitePen, center, center - armLength, center, center - gap);
                // Nhánh dưới
                g.DrawLine(whitePen, center, center + gap, center, center + armLength);

                // Điểm tâm trắng sắc nét
                using var whiteDot = new SolidBrush(Color.White);
                g.FillRectangle(whiteDot, center, center, 1, 1);
            }

            var hIcon = bitmap.GetHicon();
            if (hIcon == IntPtr.Zero)
            {
                return Cursors.Cross;
            }

            try
            {
                if (NativeMethods.GetIconInfo(hIcon, out var iconInfo))
                {
                    iconInfo.fIcon = false; // Đánh dấu là Cursor
                    iconInfo.xHotspot = center;
                    iconInfo.yHotspot = center;

                    var hCursor = NativeMethods.CreateIconIndirect(ref iconInfo);

                    if (iconInfo.hbmColor != IntPtr.Zero) NativeMethods.DeleteObject(iconInfo.hbmColor);
                    if (iconInfo.hbmMask != IntPtr.Zero) NativeMethods.DeleteObject(iconInfo.hbmMask);

                    if (hCursor != IntPtr.Zero)
                    {
                        var safeHandle = new SafeIconHandle(hCursor);
                        return CursorInteropHelper.Create(safeHandle);
                    }
                }

                // Fallback nếu không chỉnh được hotspot
                return CursorInteropHelper.Create(new SafeIconHandle(hIcon));
            }
            finally
            {
                NativeMethods.DestroyIcon(hIcon);
            }
        }
        catch
        {
            return Cursors.Cross;
        }
    }
}
