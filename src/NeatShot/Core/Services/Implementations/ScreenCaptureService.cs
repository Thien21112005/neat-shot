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
        // 1. Chuyển Foreground về Desktop Window để Windows Shell tự động đóng context menu / popup khay hệ thống một cách an toàn
        // (Tránh dùng keybd_event gửi VK_ESCAPE vì khi người dùng nhấn giữ tổ hợp Ctrl + Shift, Escape sẽ tạo thành Ctrl + Shift + Esc kích hoạt Task Manager)
        var desktopWnd = NativeMethods.GetDesktopWindow();
        if (desktopWnd != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(desktopWnd);
        }

        // 2. Ẩn chủ động cửa sổ khay hệ thống (NotifyIconOverflowWindow trên Windows 10 & 11)
        var overflowWnd = NativeMethods.FindWindow("NotifyIconOverflowWindow", null);
        if (overflowWnd != IntPtr.Zero)
        {
            NativeMethods.ShowWindow(overflowWnd, NativeConstants.SW_HIDE);
        }

        var topLevelOverflow = NativeMethods.FindWindow("TopLevelWindowForOverflowList", null);
        if (topLevelOverflow != IntPtr.Zero)
        {
            NativeMethods.ShowWindow(topLevelOverflow, NativeConstants.SW_HIDE);
        }

        // 3. Chờ 200ms để hiệu ứng animation đóng popup của Windows hoàn tất sạch sẽ
        await Task.Delay(200, cancellationToken);

        // 3. Thực hiện chụp ảnh toàn bộ toạ độ Virtual Screen qua Win32 GDI BitBlt
        var bounds = GetVirtualScreenBounds();
        var physicalLeft = NativeMethods.GetSystemMetrics(NativeConstants.SM_XVIRTUALSCREEN);
        var physicalTop = NativeMethods.GetSystemMetrics(NativeConstants.SM_YVIRTUALSCREEN);
        var physicalWidth = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXVIRTUALSCREEN);
        var physicalHeight = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYVIRTUALSCREEN);

        var hDesktopDC = NativeMethods.GetDC(IntPtr.Zero);

        if (physicalWidth <= 0 || physicalHeight <= 0)
        {
            physicalWidth = NativeMethods.GetDeviceCaps(hDesktopDC, NativeConstants.DESKTOPHORZRES);
            physicalHeight = NativeMethods.GetDeviceCaps(hDesktopDC, NativeConstants.DESKTOPVERTRES);
        }

        if (physicalWidth <= 0) physicalWidth = (int)bounds.Width;
        if (physicalHeight <= 0) physicalHeight = (int)bounds.Height;

        var hMemDC = NativeMethods.CreateCompatibleDC(hDesktopDC);
        var hBitmap = NativeMethods.CreateCompatibleBitmap(hDesktopDC, physicalWidth, physicalHeight);
        var hOld = NativeMethods.SelectObject(hMemDC, hBitmap);

        try
        {
            // Chụp kèm cờ CAPTUREBLT để chụp đúng các cửa sổ layered/trong suốt
            NativeMethods.BitBlt(
                hMemDC, 0, 0, physicalWidth, physicalHeight,
                hDesktopDC, physicalLeft, physicalTop,
                NativeConstants.SRCCOPY | NativeConstants.CAPTUREBLT);

            NativeMethods.SelectObject(hMemDC, hOld);

            // Chuyển đổi sang WPF BitmapSource
            var rawBitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Tính toán DPI thực tế dựa trên tỉ lệ physical pixels / WPF DIPs
            var dpiScaleX = bounds.Width > 0 ? (double)physicalWidth / bounds.Width : 1.0;
            var dpiScaleY = bounds.Height > 0 ? (double)physicalHeight / bounds.Height : 1.0;
            var dpiX = 96.0 * dpiScaleX;
            var dpiY = 96.0 * dpiScaleY;

            BitmapSource finalBitmap;
            if (Math.Abs(dpiX - 96.0) > 0.01 || Math.Abs(dpiY - 96.0) > 0.01)
            {
                var stride = physicalWidth * 4;
                var pixels = new byte[stride * physicalHeight];
                rawBitmapSource.CopyPixels(pixels, stride, 0);
                finalBitmap = BitmapSource.Create(
                    physicalWidth,
                    physicalHeight,
                    dpiX,
                    dpiY,
                    rawBitmapSource.Format,
                    null,
                    pixels,
                    stride);
            }
            else
            {
                finalBitmap = rawBitmapSource;
            }

            // Đóng băng (Freeze) để có thể truy cập an toàn từ mọi thread
            finalBitmap.Freeze();
            return finalBitmap;
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

        var scaleX = source.DpiX > 0 ? source.DpiX / 96.0 : 1.0;
        var scaleY = source.DpiY > 0 ? source.DpiY / 96.0 : 1.0;

        var x = (int)Math.Max(0, Math.Round(norm.X * scaleX));
        var y = (int)Math.Max(0, Math.Round(norm.Y * scaleY));

        var maxAvailableWidth = source.PixelWidth - x;
        var maxAvailableHeight = source.PixelHeight - y;

        if (maxAvailableWidth <= 0 || maxAvailableHeight <= 0)
        {
            return source;
        }

        var width = (int)Math.Min(Math.Round(norm.Width * scaleX), maxAvailableWidth);
        var height = (int)Math.Min(Math.Round(norm.Height * scaleY), maxAvailableHeight);

        if (width <= 0 || height <= 0)
        {
            return source;
        }

        var cropped = new CroppedBitmap(source, new Int32Rect(x, y, width, height));
        cropped.Freeze();
        return cropped;
    }
}
