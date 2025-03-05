namespace MuseDashModTools.Abstractions;

public interface ILocalizationService
{
    Language[] AvailableLanguages { get; }
    Language GetCurrentLanguage();
    void SetLanguage(string language);
}