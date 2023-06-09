using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using Splat;

namespace MuseDashModToolsUI.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ISettingService _settingService;

    public LocalizationService(ISettingService settingService)
    {
        _settingService = settingService;
    }

    public void SetLanguage(string language)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        _settingService.Settings.Language = language;
        Locator.Current.GetRequiredService<IMainWindowViewModel>().Refresh();
    }
}