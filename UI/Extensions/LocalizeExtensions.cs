using System;
using Avalonia.Markup.Xaml;
using MuseDashModToolsUI.Localization;

namespace MuseDashModToolsUI.Extensions;

public class LocalizeExtensions : MarkupExtension
{
    private string Key { get; }

    public LocalizeExtensions(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Resources.ResourceManager.GetString(Key, Resources.Culture)?.Replace("\\n", "\n") ?? $"#{Key}#";
    }
}