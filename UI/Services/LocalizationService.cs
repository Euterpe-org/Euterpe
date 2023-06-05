using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Localization;

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
        Resources.Culture = CultureInfo.GetCultureInfo(language);
        _settingService.Settings.Language = language;
    }
}