using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Extensions.MarkupExtensions;

public class FontExtensions : MarkupExtension
{
    private string? Key { get; }
    public static IFontManageService? FontManageService { get; set; }

    public FontExtensions(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding
    {
        Mode = BindingMode.OneWay,
        Source = FontManageService,
        Path = $"[{Key}]"
    };
}