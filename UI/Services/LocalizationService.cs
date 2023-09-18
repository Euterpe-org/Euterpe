using System.ComponentModel;
using System.Globalization;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Models;
using static MuseDashModToolsUI.Localization.Resources;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public partial class LocalizationService : ILocalizationService, INotifyPropertyChanged
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
        ResourceManager.GetString(resourceKey, Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

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
}