using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Extensions.MarkupExtensions;

public sealed class FontExtension(string key) : MarkupExtension
{
    private string Key { get; } = key;
    public static IFontManageService? FontManageService { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding
    {
        Mode = BindingMode.OneWay,
        Source = FontManageService,
        Path = $"[{Key}]"
    };
}