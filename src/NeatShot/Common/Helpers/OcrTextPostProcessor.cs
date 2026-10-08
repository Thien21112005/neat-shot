using System.Text;
using System.Text.RegularExpressions;

namespace NeatShot.Common.Helpers;

/// <summary>
/// Bộ xử lý hậu kỳ kết quả OCR từ Windows Media OCR:
/// 1. Giữ nguyên ngắt dòng giữa các dòng văn bản (không bị dồn cục thành 1 đoạn).
/// 2. Chuẩn hoá và khôi phục các ký tự tiếng Việt có dấu bị lỗi do OCR tiếng Anh (en-US)
///    như umlauts (ö -> ô, ä -> á/ă, å -> ắ), nhầm lẫn hình học (chvp -> chụp), và cụm từ tiếng Việt phổ biến.
/// </summary>
public static class OcrTextPostProcessor
{
    private static readonly (string Pattern, string Replacement)[] PhraseReplacements = new[]
    {
        (@"\bchup\s+man\s+hinh\s+thöng\s+minh\b", "chụp màn hình thông minh"),
        (@"\bchup\s+man\s+hinh\s+thong\s+minh\b", "chụp màn hình thông minh"),
        (@"\bchvp\s+man\s+hinh\s+thöng\s+minh\b", "chụp màn hình thông minh"),
        (@"\bchvp\s+man\s+hinh\s+thong\s+minh\b", "chụp màn hình thông minh"),
        (@"\bchup\s+man\s+hinh\b", "chụp màn hình"),
        (@"\bchvp\s+man\s+hinh\b", "chụp màn hình"),
        (@"\bman\s+hinh\b", "màn hình"),
        (@"\bthöng\s+minh\b", "thông minh"),
        (@"\bthong\s+minh\b", "thông minh"),
        (@"\btrang\s+thäi:", "trạng thái:"),
        (@"\btrang\s+thai:", "trạng thái:"),
        (@"\btrang\s+thäi\b", "trạng thái"),
        (@"\btrang\s+thai\b", "trạng thái"),
        (@"\bdang\s+chay\s+ngäm\b", "đang chạy ngầm"),
        (@"\bdang\s+chay\s+ngam\b", "đang chạy ngầm"),
        (@"\bdang\s+chay\b", "đang chạy"),
        (@"\bchay\s+ngäm\b", "chạy ngầm"),
        (@"\bchay\s+ngam\b", "chạy ngầm"),
        (@"\bkhay\s+he\s+thöng\b", "khay hệ thống"),
        (@"\bkhay\s+he\s+thong\b", "khay hệ thống"),
        (@"\bkhay\s+hë\s+thöng\b", "khay hệ thống"),
        (@"\bkhay\s+hệ\s+thöng\b", "khay hệ thống"),
        (@"\bhe\s+thöng\b", "hệ thống"),
        (@"\bhe\s+thong\b", "hệ thống"),
        (@"\bhë\s+thöng\b", "hệ thống"),
        (@"\bphim\s+tåt\s+chvp\b", "phím tắt chụp"),
        (@"\bphim\s+tat\s+chup\b", "phím tắt chụp"),
        (@"\bphim\s+tåt\b", "phím tắt"),
        (@"\bphim\s+tat\b", "phím tắt"),
        (@"\btåt\s+chvp\b", "tắt chụp"),
        (@"\btat\s+chup\b", "tắt chụp"),
        (@"\bhäy\s+nhän\b", "hãy nhấn"),
        (@"\bhay\s+nhan\b", "hãy nhấn"),
        (@"\bnhän\s+ctrl\b", "nhấn ctrl"),
        (@"\bnhan\s+ctrl\b", "nhấn ctrl"),
        (@"\bnéu\s+phim\b", "nếu phím"),
        (@"\bneu\s+phim\b", "nếu phím"),
        (@"\bbi\s+windows\b", "bị Windows"),
        (@"\bsnipping\s+tool\s+chän\b", "Snipping Tool chặn"),
        (@"\bsnipping\s+tool\s+chan\b", "Snipping Tool chặn"),
        (@"\*\s+meo:\b", "* Mẹo:"),
        (@"\*\s+meo\b", "* Mẹo")
    };

    private static readonly (string Pattern, string Replacement)[] WordReplacements = new[]
    {
        (@"\bchvp\b", "chụp"),
        (@"\btåt\b", "tắt"),
        (@"\bbåt\b", "bắt"),
        (@"\bcåt\b", "cắt"),
        (@"\bdåt\b", "đặt"),
        (@"\bmåt\b", "mắt"),
        (@"\bgiåt\b", "giặt"),
        (@"\bhoäc\b", "hoặc"),
        (@"\bnéu\b", "nếu"),
        (@"\bchän\b", "chặn"),
        (@"\bhäy\b", "hãy"),
        (@"\bnhän\b", "nhấn"),
        (@"\bngäm\b", "ngầm"),
        (@"\bthäi\b", "thái"),
        (@"\bthöng\b", "thông"),
        (@"\bkhöng\b", "không"),
        (@"\bcöng\b", "công"),
        (@"\btöi\b", "tôi"),
        (@"\bröi\b", "rồi"),
        (@"\btröi\b", "trời"),
        (@"\bngöi\b", "ngồi"),
        (@"\bmeo:\b", "mẹo:"),
        (@"\bdä\b", "đã"),
        (@"\bnäy\b", "này"),
        (@"\bmäy\b", "máy"),
        (@"\bchäy\b", "chạy")
    };

    /// <summary>
    /// Định dạng danh sách dòng tách biệt thành một chuỗi văn bản hoàn chỉnh với ký tự ngắt dòng.
    /// Bỏ qua các dòng trống hoặc khoảng trắng thừa.
    /// </summary>
    public static string FormatLines(IEnumerable<string>? lines)
    {
        if (lines == null) return string.Empty;

        var validLines = lines
            .Select(l => l?.Trim() ?? string.Empty)
            .Where(l => !string.IsNullOrEmpty(l));

        return string.Join(Environment.NewLine, validLines);
    }

    /// <summary>
    /// Chuẩn hoá chuỗi văn bản OCR, khôi phục tiếng Việt và các ngắt dòng hợp lý.
    /// </summary>
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        // 1. Áp dụng các mẫu cụm từ phổ biến (Case-preserving)
        foreach (var (pattern, replacement) in PhraseReplacements)
        {
            text = Regex.Replace(text, pattern, m => MatchCase(m.Value, replacement), RegexOptions.IgnoreCase);
        }

        // 2. Áp dụng các từ bị lỗi đặc trưng (Case-preserving)
        foreach (var (pattern, replacement) in WordReplacements)
        {
            text = Regex.Replace(text, pattern, m => MatchCase(m.Value, replacement), RegexOptions.IgnoreCase);
        }

        // 3. Xử lý các nguyên âm có dấu Umlaut còn lại do en-US nhận diện nhầm
        text = text.Replace('ö', 'ô').Replace('Ö', 'Ô');
        text = text.Replace('å', 'ắ').Replace('Å', 'Ắ');
        text = text.Replace('ë', 'ê').Replace('Ë', 'Ê');
        text = text.Replace('ü', 'ư').Replace('Ü', 'Ư');

        // Đối với 'ä', tuỳ theo vần:
        // - kết thúc bằng "äy" -> thường là "ãy" hoặc "ày" -> "ãy" (như hãy, máy, này)
        text = Regex.Replace(text, @"([A-Za-z]*)äy\b", "$1ãy");
        text = Regex.Replace(text, @"([A-Za-z]*)än\b", "$1ặn");
        text = Regex.Replace(text, @"([A-Za-z]*)äi\b", "$1ái");
        text = Regex.Replace(text, @"([A-Za-z]*)äm\b", "$1ầm");
        text = Regex.Replace(text, @"([A-Za-z]*)äc\b", "$1ặc");

        // Các chữ 'ä' còn sót lại chuyển về 'ă' hoặc 'a'
        text = text.Replace('ä', 'ă').Replace('Ä', 'Ă');

        return text;
    }

    /// <summary>
    /// Giữ nguyên kiểu viết hoa chữ cái đầu hoặc viết hoa toàn bộ của chuỗi gốc khi thay thế.
    /// </summary>
    private static string MatchCase(string original, string replacement)
    {
        if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replacement))
        {
            return replacement;
        }

        // Nếu chuỗi gốc viết hoa toàn bộ (ví dụ: CHVP)
        if (original.Length > 1 && original.All(c => !char.IsLetter(c) || char.IsUpper(c)))
        {
            return replacement.ToUpper();
        }

        // Nếu chuỗi gốc viết hoa chữ cái đầu (ví dụ: Chvp, Néu)
        if (char.IsUpper(original[0]))
        {
            var chars = replacement.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        return replacement;
    }
}
