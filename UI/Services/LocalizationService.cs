using System.ComponentModel;
using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using Splat;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ISettingService _settingService;

    public string this[string resourceKey] => ResourceManager.GetString(resourceKey, Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

    public LocalizationService(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public void SetLanguage(string language)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        Locator.Current.GetRequiredService<IMainWindowViewModel>().ChangeTabName();
        _settingService.Settings.Language = language;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}