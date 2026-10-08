using NeatShot.Common.Helpers;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class OcrTextPostProcessorTests
{
    [Fact]
    public void FormatLines_PreservesMultipleLines_WithNewlineSeparators()
    {
        var rawLines = new[]
        {
            "Line 1: Header",
            "Line 2: Content",
            "Line 3: Footer"
        };

        var result = OcrTextPostProcessor.FormatLines(rawLines);

        var expected = string.Join(Environment.NewLine, rawLines);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatLines_IgnoresEmptyOrWhitespaceLines()
    {
        var rawLines = new[]
        {
            "Line 1",
            "   ",
            "",
            "Line 2"
        };

        var result = OcrTextPostProcessor.FormatLines(rawLines);

        var expected = $"Line 1{Environment.NewLine}Line 2";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_FixesVietnameseOcrArtifacts_FromUserReportedSample()
    {
        // Chuỗi thực tế người dùng gặp phải từ en-US OCR trên Windows:
        var input = "NeatShot - Chup man hinh thöng minh Trang thäi: Dang chay ngäm trong khay he thöng (System Tray) phim tåt chvp: Ctrl + Shift + A hoäc printscreen * meo: Néu phim printscreen bi Windows Snipping Tool chän, häy nhän Ctrl + Shift + A hoäc click icon NeatShot khay he thöng.";

        var normalized = OcrTextPostProcessor.Normalize(input);

        // Các lỗi nhận diện tiếng Việt đặc trưng phải được sửa chính xác:
        Assert.Contains("Chụp màn hình thông minh", normalized);
        Assert.Contains("Trạng thái:", normalized);
        Assert.Contains("Đang chạy ngầm trong khay hệ thống", normalized);
        Assert.Contains("phím tắt chụp:", normalized);
        Assert.Contains("hoặc printscreen", normalized);
        Assert.Contains("* Mẹo:", normalized);
        Assert.Contains("Nếu phím printscreen bị Windows Snipping Tool chặn,", normalized);
        Assert.Contains("hãy nhấn", normalized);
        Assert.Contains("khay hệ thống", normalized);

        // Các từ tiếng Anh và phím tắt kỹ thuật phải được giữ nguyên:
        Assert.Contains("System Tray", normalized);
        Assert.Contains("Ctrl + Shift + A", normalized);
        Assert.Contains("Windows Snipping Tool", normalized);
        Assert.Contains("NeatShot", normalized);
    }

    [Theory]
    [InlineData("thöng minh", "thông minh")]
    [InlineData("khöng", "không")]
    [InlineData("cöng cụ", "công cụ")]
    [InlineData("tåt", "tắt")]
    [InlineData("båt", "bắt")]
    [InlineData("chvp", "chụp")]
    [InlineData("hoäc", "hoặc")]
    [InlineData("Néu", "Nếu")]
    [InlineData("häy", "hãy")]
    [InlineData("chän", "chặn")]
    [InlineData("thäi", "thái")]
    [InlineData("ngäm", "ngầm")]
    public void Normalize_FixesSpecificUmlautCorruptedVietnameseWords(string input, string expected)
    {
        var result = OcrTextPostProcessor.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_PreservesEnglishAndCodeSnippets()
    {
        var input = "public void TestMethod() { var x = 10; return; }";
        var result = OcrTextPostProcessor.Normalize(input);
        Assert.Equal(input, result);
    }

    [Fact]
    public void Normalize_FixesSlashedZeroAndHexCodeArtifacts()
    {
        var input = "NIM_ADD = øxøøeøøeøø; NIM_MODIFY = øxøøeøøøøl; NIF_MESSAGE = exøeøøøøel; Shell Notifylcon";
        var normalized = OcrTextPostProcessor.Normalize(input);

        Assert.Contains("0x00000000;", normalized);
        Assert.Contains("0x00000001;", normalized);
        Assert.Contains("NotifyIcon", normalized);
    }
}
