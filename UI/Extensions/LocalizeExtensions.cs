using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MuseDashModToolsUI.Contracts;

namespace MuseDashModToolsUI.Extensions;

public class LocalizeExtensions : MarkupExtension
{
    private string? Key { get; }
    public static ILocalizationService? LocalizationService { get; set; }

    public LocalizeExtensions(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider) => new Binding
    {
        Mode = BindingMode.OneWay,
        Source = LocalizationService,
        Path = $"[{Key}]"
    };
}