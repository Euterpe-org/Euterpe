namespace MuseDashModTools.Abstractions;

public interface ILocalizationService
{
    Language[] AvailableLanguages { get; }
    int GetCurrentLanguageIndex();
    void SetLanguage(string language);
}