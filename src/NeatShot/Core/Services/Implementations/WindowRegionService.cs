using NeatShot.Common.Helpers;
using NeatShot.Core.Interop;
using NeatShot.Core.Models;
using NeatShot.Core.Services.Interfaces;
using System.Runtime.InteropServices;
using System.Windows;

namespace NeatShot.Core.Services.Implementations;

/// <summary>
/// Liệt kê cửa sổ bằng EnumWindows / EnumChildWindows, lấy khung thật (không tính bóng đổ) qua DWM.
/// Bỏ qua cửa sổ ẩn, thu nhỏ, bị "cloak" (UWP ẩn) và cửa sổ của chính NeatShot.
/// </summary>
public class WindowRegionService : IWindowRegionService
{
    private const int MinSize = 8;
    private const int MaxChildrenPerWindow = 200;

    public IReadOnlyList<WindowRegion> GetVisibleWindows()
    {
        var ownProcessId = (uint)Environment.ProcessId;
        var topLevels = new List<(IntPtr Handle, Rect Bounds)>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (IsCandidate(hWnd, ownProcessId) && TryGetBounds(hWnd, out var bounds))
            {
                topLevels.Add((hWnd, bounds));
            }
            return true;
        }, IntPtr.Zero);

        var regions = new List<WindowRegion>();
        for (var index = 0; index < topLevels.Count; index++)
        {
            var (handle, bounds) = topLevels[index];
            regions.Add(new WindowRegion(bounds, index, IsChild: false));

            var children = 0;
            NativeMethods.EnumChildWindows(handle, (child, _) =>
            {
                if (NativeMethods.IsWindowVisible(child)
                    && NativeMethods.GetWindowRect(child, out var rect)
                    && rect.Width >= MinSize && rect.Height >= MinSize)
                {
                    var childBounds = Rect.Intersect(ToRect(rect), bounds);
                    if (!childBounds.IsEmpty && childBounds != bounds)
                    {
                        regions.Add(new WindowRegion(childBounds, index, IsChild: true));
                    }
                }
                return ++children < MaxChildrenPerWindow;
            }, IntPtr.Zero);
        }

        return regions;
    }

    public IReadOnlyList<WindowRegion> GetOverlayRegions(double scaleX, double scaleY)
    {
        var originX = NativeMethods.GetSystemMetrics(NativeConstants.SM_XVIRTUALSCREEN);
        var originY = NativeMethods.GetSystemMetrics(NativeConstants.SM_YVIRTUALSCREEN);

        return GetVisibleWindows()
            .Select(r => r with { Bounds = WindowHitTester.ToOverlay(r.Bounds, originX, originY, scaleX, scaleY) })
            .ToList();
    }

    private static bool IsCandidate(IntPtr hWnd, uint ownProcessId)
    {
        if (!NativeMethods.IsWindowVisible(hWnd) || NativeMethods.IsIconic(hWnd)) return false;

        NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);
        if (processId == ownProcessId) return false;

        var cloakedSize = Marshal.SizeOf<int>();
        if (NativeMethods.DwmGetWindowAttribute(hWnd, NativeConstants.DWMWA_CLOAKED, out int cloaked, cloakedSize) == 0 && cloaked != 0)
        {
            return false;
        }

        return true;
    }

    private static bool TryGetBounds(IntPtr hWnd, out Rect bounds)
    {
        bounds = Rect.Empty;
        var hasFrame = NativeMethods.DwmGetWindowAttribute(
            hWnd, NativeConstants.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT frame, Marshal.SizeOf<RECT>()) == 0;

        if (!hasFrame && !NativeMethods.GetWindowRect(hWnd, out frame)) return false;
        if (frame.Width < MinSize || frame.Height < MinSize) return false;

        bounds = ToRect(frame);
        return true;
    }

    private static Rect ToRect(RECT rect) => new(rect.Left, rect.Top, rect.Width, rect.Height);
}
