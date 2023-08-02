using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Localization;
using MuseDashModToolsUI.Models;
using Serilog;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ILogger _logger;
    public ISavingService SavingService { get; init; }
    public Lazy<IUpdateTextService> UpdateTextService { get; init; }

    public LocalizationService(ILogger logger)
    {
        _logger = logger;
        GetAvailableCultures();
    }

    public string this[string resourceKey] =>
        Resources.ResourceManager.GetString(resourceKey, Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

    public List<Language> AvailableLanguages { get; } = new();

    public void SetLanguage(string language)
    {
        if (CultureInfo.CurrentUICulture.Name == language) return;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        SavingService.Settings.LanguageCode = language;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        UpdateTextService.Value.UpdateText();
        _logger.Information("Language changed to {Language}", language);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void GetAvailableCultures()
    {
        var rm = new ResourceManager(typeof(Resources));
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var defaultCulture = CultureInfo.GetCultureInfo("en");

        foreach (var culture in cultures)
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
            {
                AvailableLanguages.Add(new Language(defaultCulture));
                continue;
            }

            var rs = rm.GetResourceSet(culture, true, false);
            if (rs != null)
                AvailableLanguages.Add(new Language(culture));
        }

        _logger.Information("Available languages loaded: {AvailableLanguages}",
            string.Join(", ", AvailableLanguages.Select(x => x.Name)));
    }
}