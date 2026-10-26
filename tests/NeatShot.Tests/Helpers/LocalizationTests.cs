using NeatShot.Common.Localization;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace NeatShot.Tests.Helpers;

public class LocalizationTests
{
    [Theory]
    [InlineData("vi", "vi")]
    [InlineData("en", "en")]
    [InlineData(null, "vi")]
    [InlineData("fr", "vi")]
    public void Normalize_DefaultsToVietnamese(string? choice, string expected)
    {
        Assert.Equal(expected, Loc.Normalize(choice));
    }

    [Fact]
    public void Translate_ReturnsTheRequestedLanguage()
    {
        Assert.Equal("Thoát", Loc.Translate("Tray.Exit", "vi"));
        Assert.Equal("Exit", Loc.Translate("Tray.Exit", "en"));
    }

    [Fact]
    public void Translate_ReturnsTheKey_WhenItIsMissing()
    {
        Assert.Equal("No.Such.Key", Loc.Translate("No.Such.Key", "en"));
    }

    [Fact]
    public void StringTable_HasUniqueKeys_AndBothLanguagesForEveryKey()
    {
        var duplicates = Strings.Table.GroupBy(e => e.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);

        foreach (var entry in Strings.Table)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Vi), $"{entry.Key} thiếu tiếng Việt");
            Assert.False(string.IsNullOrWhiteSpace(entry.En), $"{entry.Key} thiếu tiếng Anh");
        }
    }

    [Fact]
    public void FormatStrings_HaveTheSamePlaceholders_InBothLanguages()
    {
        var placeholder = new Regex(@"\{\d+\}");
        foreach (var entry in Strings.Table)
        {
            var vi = placeholder.Matches(entry.Vi).Select(m => m.Value).Order();
            var en = placeholder.Matches(entry.En).Select(m => m.Value).Order();
            Assert.True(vi.SequenceEqual(en), $"{entry.Key}: tham số khác nhau giữa hai ngôn ngữ");
        }
    }

    [Fact]
    public void EveryKeyUsedInTheApp_ExistsInTheStringTable()
    {
        var src = FindSourceFolder();
        var used = new Regex(@"\{l:T\s+([\w.]+)\}|Loc\.[TF]\(\s*""([\w.]+)""");
        var known = Strings.Table.Select(e => e.Key).ToHashSet();
        var missing = new List<string>();

        foreach (var file in Directory.EnumerateFiles(src, "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".xaml") || f.EndsWith(".cs"))
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                              && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")))
        {
            foreach (Match m in used.Matches(File.ReadAllText(file)))
            {
                var key = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!known.Contains(key)) missing.Add($"{Path.GetFileName(file)}: {key}");
            }
        }

        Assert.Empty(missing);
    }

    private static string FindSourceFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "NeatShot")))
        {
            dir = dir.Parent;
        }

        // Thư mục build có thể nằm ngoài repo (--artifacts-path): dùng vị trí file test lúc biên dịch
        return dir != null
            ? Path.Combine(dir.FullName, "src", "NeatShot")
            : Path.GetFullPath(Path.Combine(ThisFileDirectory(), "..", "..", "..", "src", "NeatShot"));
    }

    private static string ThisFileDirectory([System.Runtime.CompilerServices.CallerFilePath] string path = "") =>
        Path.GetDirectoryName(path)!;
}
