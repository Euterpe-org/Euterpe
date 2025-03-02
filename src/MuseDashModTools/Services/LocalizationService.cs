using Avalonia.Markup.Xaml.MarkupExtensions;
using Ursa.Themes.Semi;

namespace MuseDashModTools.Services;

internal sealed class LocalizationService : ILocalizationService
{
    public Language[] AvailableLanguages { get; } =
    [
        new("en"),
        new("de"),
        new("es"),
        new("fr"),
        new("hr"),
        new("hu"),
        new("ja"),
        new("ko"),
        new("ru"),
        new("zh-Hans"),
        new("zh-Hant")
    ];

    public int GetCurrentLanguageIndex()
    {
        CultureInfo currentCulture;

        try
        {
            currentCulture = CultureInfo.GetCultureInfo(Config.LanguageCode);
        }
        catch (CultureNotFoundException ex)
        {
            currentCulture = CultureInfo.CurrentUICulture;
            Logger.ZLogError(ex, $"Invalid language code {Config.LanguageCode} from config, falling back to {currentCulture.EnglishName}");
        }

        foreach (var cultureName in CreateCultureFallbackChain(currentCulture).Select(x => x.Name))
        {
            var index = Array.FindIndex(AvailableLanguages, x => x.Name == cultureName);
            if (index is -1)
            {
                continue;
            }

            Config.LanguageCode = cultureName;
            return index;
        }

        Logger.ZLogError($"No matching language found for {currentCulture.Name}, falling back to English");
        Config.LanguageCode = "en";
        return 0;
    }

    public void SetLanguage(string language)
    {
        if (CultureInfo.CurrentUICulture.Name == language)
        {
            return;
        }

        var culture = CultureInfo.GetCultureInfo(language);
        I18NExtension.Culture = culture;
        CultureInfo.CurrentUICulture = culture;
        SemiTheme.OverrideLocaleResources(GetCurrentApplication(), culture);

        Config.LanguageCode = language;
        Logger.ZLogInformation($"Language set to {language}");
    }

    private static IEnumerable<CultureInfo> CreateCultureFallbackChain(CultureInfo startingCulture)
    {
        var current = startingCulture;
        while (current.Name != CultureInfo.InvariantCulture.Name)
        {
            yield return current;
            current = current.Parent;
        }

        yield return CultureInfo.InvariantCulture;
    }


    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required ILogger<LocalizationService> Logger { get; init; }

    #endregion Injections
}