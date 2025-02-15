namespace MuseDashModTools.Core;

internal sealed partial class SavingService : ISavingService
{
    private const string ConfigFileName = "Config.json";
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuseDashModTools));
    private static readonly string ConfigPath = Path.Combine(ConfigFolder, ConfigFileName);

    public async Task LoadSettingAsync()
    {
        if (File.Exists(ConfigPath))
        {
            await using var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            var savedConfig = await JsonSerializationService.DeserializeIndentedAsync<Config>(stream).ConfigureAwait(true);
            if (savedConfig is null)
            {
                Logger.ZLogError($"Saved setting is null");
                await MessageBoxService.ErrorMessageBoxAsync("Failed to load setting, please delete the setting file and restart the application")
                    .ConfigureAwait(true);
                PlatformService.RevealFile(ConfigPath);
                return;
            }

            Config.CopyFrom(savedConfig);
            Logger.ZLogInformation($"Setting loaded from {ConfigPath} successfully");
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
        await using var stream = new FileStream(ConfigPath, FileMode.OpenOrCreate, FileAccess.Write);
        await JsonSerializationService.SerializeIndentedAsync(stream, Config).ConfigureAwait(false);
        Logger.ZLogInformation($"Setting saved to {ConfigPath} successfully");
    }

    #region Injections

    [UsedImplicitly]
    public Config Config { get; init; } = null!;

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

    #endregion Injections
}