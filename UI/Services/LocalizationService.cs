using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Localization;
using MuseDashModToolsUI.Models;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ISettingService _settingService;

    public string this[string resourceKey] =>
        Resources.ResourceManager.GetString(resourceKey, Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

    public LocalizationService(ISettingService settingService)
    {
        _settingService = settingService;
        GetAvailableCultures();
    }

    public List<Language> AvailableLanguages { get; } = new();


    public void SetLanguage(string language)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        Locator.Current.GetRequiredService<IMainWindowViewModel>().ChangeTabName();
        _settingService.Settings.LanguageCode = language;
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
                AvailableLanguages.Add(new Language(defaultCulture.Name, defaultCulture.DisplayName));
                continue;
            }

            var rs = rm.GetResourceSet(culture, true, false);
            if (rs != null)
                AvailableLanguages.Add(new Language(culture.Name, culture.DisplayName));
        }
    }
}