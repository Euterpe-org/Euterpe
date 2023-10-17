namespace MuseDashModToolsUI.Contracts;

public interface ILocalizationService
{
    List<Language> AvailableLanguages { get; }

    /// <summary>
    ///     Set Language
    /// </summary>
    /// <param name="language"></param>
    void SetLanguage(string language);
}