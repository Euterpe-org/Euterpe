using System.ComponentModel;
using System.Globalization;
using MuseDashModToolsUI.Localization.XAML;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MuseDashModToolsUI.Services;

public sealed partial class LocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ILogger _logger;

    [UsedImplicitly]
    public ISavingService SavingService { get; init; }

    [UsedImplicitly]
    public Lazy<IUpdateUIService> UpdateTextService { get; init; }

    public string this[string resourceKey] =>
        Resources_XAML.ResourceManager.GetString(resourceKey, Resources_XAML.Culture)?.Replace("\\n", "\n") ?? $"#{resourceKey}#";

    public LocalizationService(ILogger logger)
    {
        _logger = logger;
        GetAvailableCultures();
    }

    public List<Language> AvailableLanguages { get; } = new();

    public void SetLanguage(string language)
    {
        if (CultureInfo.CurrentUICulture.Name == language)
        {
            return;
        }

        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        SavingService.Settings.LanguageCode = language;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        UpdateTextService.Value.UpdateText();
        _logger.Information("Language changed to {Language}", language);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}