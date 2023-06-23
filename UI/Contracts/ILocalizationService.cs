using System.Collections.Generic;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalizationService
{
    string this[string resourceKey] { get; }
    List<Language> AvailableLanguages { get; }
    void SetLanguage(string language);
}