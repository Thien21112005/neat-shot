using System.Globalization;

namespace NeatShot.Common.Localization;

/// <summary>
/// Ngôn ngữ giao diện của NeatShot: Tiếng Việt (mặc định) hoặc English.
/// Chuỗi lấy từ bảng <see cref="Strings.Table"/>; cửa sổ mở sau khi đổi ngôn ngữ sẽ dùng ngôn ngữ mới.
/// </summary>
public static class Loc
{
    public const string Vietnamese = "vi";
    public const string English = "en";

    private static readonly Dictionary<string, (string Vi, string En)> Lookup =
        Strings.Table.ToDictionary(e => e.Key, e => (e.Vi, e.En));

    /// <summary>
    /// Ngôn ngữ đang dùng: "vi" hoặc "en".
    /// </summary>
    public static string Language { get; private set; } = Vietnamese;

    /// <summary>
    /// Phát ra sau khi đổi ngôn ngữ (ví dụ để dựng lại menu khay).
    /// </summary>
    public static event EventHandler? LanguageChanged;

    /// <summary>
    /// Chuẩn hoá lựa chọn trong cài đặt: chỉ "en" là English, mọi giá trị khác là Tiếng Việt.
    /// </summary>
    public static string Normalize(string? choice) => choice == English ? English : Vietnamese;

    public static void SetLanguage(string? choice)
    {
        var language = Normalize(choice);
        if (language == Language) return;

        Language = language;
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Chuỗi theo ngôn ngữ hiện tại.
    /// </summary>
    public static string T(string key) => Translate(key, Language);

    /// <summary>
    /// Chuỗi có tham số ({0}, {1}…) theo ngôn ngữ hiện tại.
    /// </summary>
    public static string F(string key, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(key), args);

    /// <summary>
    /// Chuỗi theo ngôn ngữ chỉ định; trả về chính khoá nếu không có trong bảng (test chặn trường hợp này).
    /// </summary>
    public static string Translate(string key, string language)
    {
        if (!Lookup.TryGetValue(key, out var entry)) return key;
        return Normalize(language) == English ? entry.En : entry.Vi;
    }
}
