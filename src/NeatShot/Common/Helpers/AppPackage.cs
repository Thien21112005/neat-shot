using NeatShot.Core.Interop;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Cho biết NeatShot đang chạy từ gói MSIX (cài qua Microsoft Store) hay bản portable (file zip).
/// </summary>
public static class AppPackage
{
    private const int AppModelErrorNoPackage = 15700;

    private static readonly Lazy<bool> _isPackaged = new(DetectPackaged);

    /// <summary>
    /// true nếu tiến trình chạy trong gói MSIX.
    /// </summary>
    public static bool IsPackaged => _isPackaged.Value;

    private static bool DetectPackaged()
    {
        try
        {
            var length = 0;
            return NativeMethods.GetCurrentPackageFullName(ref length, null) != AppModelErrorNoPackage;
        }
        catch (EntryPointNotFoundException)
        {
            // Windows quá cũ, không có API gói ứng dụng
            return false;
        }
    }
}
