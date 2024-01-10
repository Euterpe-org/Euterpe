using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Extensions.MarkupExtensions;

public sealed class LocalizeExtension(string key) : MarkupExtension
{
    private string Key { get; } = key;
    public static ILocalizationService? LocalizationService { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding
    {
        Mode = BindingMode.OneWay,
        Source = LocalizationService,
        Path = $"[{Key}]"
    };
}