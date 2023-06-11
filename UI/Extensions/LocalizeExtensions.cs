using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MuseDashModToolsUI.Contracts;
using Splat;

namespace MuseDashModToolsUI.Extensions;

public class LocalizeExtensions : MarkupExtension
{
    private string Key { get; }

    public LocalizeExtensions(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Source = Locator.Current.GetService<ILocalizationService>(),
            Path = $"[{Key}]"
        };
    }
}