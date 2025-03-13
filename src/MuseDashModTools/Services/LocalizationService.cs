using Avalonia.Markup.Xaml.MarkupExtensions;
using Ursa.Themes.Semi;

namespace MuseDashModTools.Services;

public sealed class LocalizationService
{
    public Language[] AvailableLanguages { get; } =
    [
        "en",
        "de",
        "es",
        "fr",
        "hr",
        "hu",
        "ja",
        "ko",
        "ru",
        "zh-Hans",
        "zh-Hant"
    ];

    public Language GetCurrentLanguage()
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
            var language = AvailableLanguages.FirstOrDefault(x => x.Name == cultureName);
            if (language is null)
            {
                continue;
            }

            Config.LanguageCode = cultureName;
            return language;
        }

        Logger.ZLogError($"No matching language found for {currentCulture.Name}, falling back to English");
        Config.LanguageCode = "en";
        return "en";
    }

    public void SetLanguage(string language)
    {
        if (CultureInfo.CurrentUICulture.Name == language)
        {
            return;
        }

        var culture = CultureInfo.GetCultureInfo(language);
        CultureInfo.CurrentUICulture = culture;
        LocalizationManager.Culture = culture;
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