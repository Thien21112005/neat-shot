using System.Windows.Markup;

namespace NeatShot.Common.Localization;

/// <summary>
/// Lấy chuỗi giao diện trong XAML: <c>Text="{l:T Settings.Title}"</c>
/// (với <c>xmlns:l="clr-namespace:NeatShot.Common.Localization"</c>).
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class TExtension : MarkupExtension
{
    public TExtension()
    {
    }

    public TExtension(string key)
    {
        Key = key;
    }

    [ConstructorArgument("key")]
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider) => Loc.T(Key);
}
