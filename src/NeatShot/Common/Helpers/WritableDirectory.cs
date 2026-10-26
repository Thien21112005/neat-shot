using System.IO;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Kiểm tra một thư mục có thật sự ghi được không. Windows Defender "Controlled folder access"
/// (Windows Security → Ransomware protection) chặn app lạ ghi vào Pictures, Documents, Desktop…
/// mà không báo gì cho người dùng, nên phải thử ghi thật thay vì chỉ kiểm tra thư mục tồn tại.
/// </summary>
public static class WritableDirectory
{
    /// <summary>
    /// Thư mục dự phòng không thuộc nhóm thư mục được bảo vệ: %USERPROFILE%\NeatShot.
    /// (Không dùng AppData: bản MSIX bị ảo hoá AppData nên Explorer không mở được đúng chỗ.)
    /// </summary>
    public static string DefaultFallback =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "NeatShot");

    /// <summary>
    /// Tạo thư mục (nếu chưa có) rồi thử ghi và xoá một file nhỏ.
    /// </summary>
    /// <returns>true nếu ghi được.</returns>
    public static bool TryPrepare(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return false;

        try
        {
            Directory.CreateDirectory(directory);
            var probe = Path.Combine(directory, $".neatshot-write-test-{Guid.NewGuid():N}");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            System.Diagnostics.Debug.WriteLine($"[WritableDirectory] Không ghi được vào {directory}: {ex.Message}");
            return false;
        }
    }
}
