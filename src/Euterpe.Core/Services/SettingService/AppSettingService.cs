namespace Euterpe.Core;

internal sealed partial class AppSettingService : IAppSettingService
{
    private const string ConfigFileName = "Config.json";
    private static readonly string ConfigPath = Path.Combine(AppDataFolder, ConfigFileName);

    public void Load()
    {
        if (File.Exists(ConfigPath))
        {
            using var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            var savedConfig = JsonSerializationService.DeserializeConfig(stream);
            if (savedConfig is null)
            {
                Logger.ZLogError($"Saved setting is null, using default settings");
                return;
            }

            Config.CopyFrom(savedConfig);
            Logger.ZLogInformation($"Setting loaded from {ConfigPath} successfully");
        }
        else
        {
            Logger.ZLogInformation($"Setting file not found, using default settings");
        }
    }

    public async Task LoadAsync()
    {
        if (File.Exists(ConfigPath))
        {
            var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            await using (stream.ConfigureAwait(false))
            {
                var savedConfig = await JsonSerializationService.DeserializeConfigAsync(stream).ConfigureAwait(true);
                if (savedConfig is null)
                {
                    Logger.ZLogError($"Saved setting is null, using default settings");
                    return;
                }

                Config.CopyFrom(savedConfig);
                Logger.ZLogInformation($"Setting loaded from {ConfigPath} successfully");
            }
        }
        else
        {
            Logger.ZLogInformation($"Setting file not found, using default settings");
        }
    }

    public void Save()
    {
        using var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        JsonSerializationService.SerializeConfig(stream, Config);
        Logger.ZLogInformation($"Setting saved to {ConfigPath} successfully");
    }

    public async Task SaveAsync()
    {
        var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        await using (stream.ConfigureAwait(false))
        {
            await JsonSerializationService.SerializeConfigAsync(stream, Config).ConfigureAwait(false);
            Logger.ZLogInformation($"Setting saved to {ConfigPath} successfully");
        }
    }

    public async Task ValidateSteamAsync()
    {
        Logger.ZLogInformation($"Checking for valid Steam setting...");

        await CheckSteamFolderAsync().ConfigureAwait(true);
        await CheckSteamExecPathAsync().ConfigureAwait(true);

        Logger.ZLogInformation($"Steam setting validated");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IJsonSerializationService JsonSerializationService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppSettingService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}