namespace MuseDashModTools.Core;

internal sealed partial class SavingService : ISavingService
{
    private const string SettingFileName = "Setting.json";
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuseDashModTools));
    private static readonly string SettingPath = Path.Combine(ConfigFolder, SettingFileName);

    public async Task LoadSettingAsync()
    {
        if (File.Exists(SettingPath))
        {
            await using var stream = new FileStream(SettingPath, FileMode.Open, FileAccess.Read);
            var savedSetting = await JsonSerializationService.DeserializeIndentedAsync<Setting>(stream).ConfigureAwait(true);
            if (savedSetting is null)
            {
                Logger.ZLogError($"Saved setting is null");
                await MessageBoxService.ErrorMessageBoxAsync("Failed to load setting, please delete the setting file and restart the application")
                    .ConfigureAwait(true);
                PlatformService.RevealFile(SettingPath);
                return;
            }

            Setting.CopyFrom(savedSetting);
            Logger.ZLogInformation($"Setting loaded from {SettingPath} successfully");
            await CheckValidSettingAsync().ConfigureAwait(true);
        }
        else
        {
            Logger.ZLogInformation($"Setting file not found, using default settings");
            await CheckValidSettingAsync().ConfigureAwait(true);
        }
    }

    public async Task SaveSettingAsync()
    {
        await using var stream = new FileStream(SettingPath, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializationService.SerializeIndentedAsync(stream, Setting).ConfigureAwait(false);
        Logger.ZLogInformation($"Setting saved to {SettingPath} successfully");
    }

    #region Injections

    [UsedImplicitly]
    public IJsonSerializationService JsonSerializationService { get; init; } = null!;

    [UsedImplicitly]
    public ILocalService LocalService { get; init; } = null!;

    [UsedImplicitly]
    public ILogger<SavingService> Logger { get; init; } = null!;

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; } = null!;

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; } = null!;

    [UsedImplicitly]
    public Setting Setting { get; init; } = null!;

    #endregion Injections
}