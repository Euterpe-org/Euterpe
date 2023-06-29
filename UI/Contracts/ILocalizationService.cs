using System.Collections.Generic;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalizationService
{
    string this[string resourceKey] { get; }
    List<Language> AvailableLanguages { get; }
    string[] AvailableFonts { get; }
    void SetLanguage(string language);
    void SetFont(string fontName);
}