using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalizationService
{
    string this[string resourceKey] { get; }
    List<Language> AvailableLanguages { get; }

    /// <summary>
    ///     Set Language
    /// </summary>
    /// <param name="language"></param>
    void SetLanguage(string language);
}