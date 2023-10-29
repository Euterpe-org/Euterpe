using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MuseDashModToolsUI.Extensions.MarkupExtensions;

public class LocalizeExtension : MarkupExtension
{
    private string Key { get; }
    public static ILocalizationService? LocalizationService { get; set; }

    public LocalizeExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding
    {
        Mode = BindingMode.OneWay,
        Source = LocalizationService,
        Path = $"[{Key}]"
    };
}