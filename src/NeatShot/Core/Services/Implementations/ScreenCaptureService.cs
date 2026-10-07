using NeatShot.Core.Interop;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Dịch vụ chụp ảnh màn hình bằng Win32 GDI BitBlt kết hợp thuật toán "Chụp sạch" (Clean Shot)
/// và xử lý cắt vùng chọn đa màn hình.
/// </summary>
public class ScreenCaptureService : IScreenCaptureService
{
    public Rect GetVirtualScreenBounds()
    {
        var width = SystemParameters.VirtualScreenWidth;
        var height = SystemParameters.VirtualScreenHeight;

        // Fallback an toàn nếu chạy trong unit test runner không có session desktop
        if (width <= 0) width = SystemParameters.PrimaryScreenWidth > 0 ? SystemParameters.PrimaryScreenWidth : 1920;
        if (height <= 0) height = SystemParameters.PrimaryScreenHeight > 0 ? SystemParameters.PrimaryScreenHeight : 1080;

        return new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            width,
            height);
    }

    public async Task<BitmapSource> CaptureCleanScreenAsync(CancellationToken cancellationToken = default)
    {
        // 1. Gửi tín hiệu để Windows chủ động ẩn popup khay hệ thống / context menu đang mở
        var desktopHwnd = NativeMethods.GetDesktopWindow();
        if (desktopHwnd != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(desktopHwnd);
        }

        // 2. Chờ 120ms để hiệu ứng animation đóng popup của Windows hoàn tất
        await Task.Delay(120, cancellationToken);

        // 3. Thực hiện chụp ảnh toàn bộ toạ độ Virtual Screen qua Win32 GDI BitBlt
        var bounds = GetVirtualScreenBounds();
        var left = (int)bounds.Left;
        var top = (int)bounds.Top;
        var width = (int)bounds.Width;
        var height = (int)bounds.Height;

        var hDesktopDC = NativeMethods.GetDC(IntPtr.Zero);
        var hMemDC = NativeMethods.CreateCompatibleDC(hDesktopDC);
        var hBitmap = NativeMethods.CreateCompatibleBitmap(hDesktopDC, width, height);
        var hOld = NativeMethods.SelectObject(hMemDC, hBitmap);

        try
        {
            // Chụp kèm cờ CAPTUREBLT để chụp đúng các cửa sổ layered/trong suốt
            NativeMethods.BitBlt(
                hMemDC, 0, 0, width, height,
                hDesktopDC, left, top,
                NativeConstants.SRCCOPY | NativeConstants.CAPTUREBLT);

            NativeMethods.SelectObject(hMemDC, hOld);

            // Chuyển đổi sang WPF BitmapSource
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Đóng băng (Freeze) để có thể truy cập an toàn từ mọi thread
            bitmapSource.Freeze();
            return bitmapSource;
        }
        finally
        {
            // Giải phóng triệt để tài nguyên GDI Handle, chống rò rỉ bộ nhớ
            if (hBitmap != IntPtr.Zero) NativeMethods.DeleteObject(hBitmap);
            if (hMemDC != IntPtr.Zero) NativeMethods.DeleteDC(hMemDC);
            if (hDesktopDC != IntPtr.Zero) NativeMethods.ReleaseDC(IntPtr.Zero, hDesktopDC);
        }
    }

    public BitmapSource Crop(BitmapSource source, CaptureRegion region)
    {
        ArgumentNullException.ThrowIfNull(source);

        var norm = region.Normalize();
        if (norm.IsEmpty)
        {
            return source;
        }

        var x = (int)Math.Max(0, norm.X);
        var y = (int)Math.Max(0, norm.Y);

        var maxAvailableWidth = source.PixelWidth - x;
        var maxAvailableHeight = source.PixelHeight - y;

        if (maxAvailableWidth <= 0 || maxAvailableHeight <= 0)
        {
            return source;
        }

        var width = (int)Math.Min(norm.Width, maxAvailableWidth);
        var height = (int)Math.Min(norm.Height, maxAvailableHeight);

        if (width <= 0 || height <= 0)
        {
            return source;
        }

        var cropped = new CroppedBitmap(source, new Int32Rect(x, y, width, height));
        cropped.Freeze();
        return cropped;
    }
}
