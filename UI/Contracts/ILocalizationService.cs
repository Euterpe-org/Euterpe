using System.Collections.Generic;
using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ILocalizationService
{
    List<Language> AvailableLanguages { get; }
    void SetLanguage(string language);
}