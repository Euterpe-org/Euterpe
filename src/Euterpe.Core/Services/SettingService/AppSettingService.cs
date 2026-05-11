namespace Euterpe.Core;

internal sealed partial class AppSettingService : IAppSettingService
{
    private const string ConfigFileName = "Config.json";
    private static readonly string ConfigPath = Path.Combine(AppDataFolder, ConfigFileName);

    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            Logger.ZLogInformation($"Setting file not found, using default settings");
            return;
        }

        Config? savedConfig;
        try
        {
            using var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            savedConfig = JsonSerializationService.DeserializeConfig(stream);
        }
        catch (Exception ex)
        {
            BackupCorruptedConfig(ex);
            return;
        }

        if (savedConfig is null)
        {
            Logger.ZLogError($"Saved setting is null, using default settings");
            return;
        }

        Config.CopyFrom(savedConfig);
        Logger.ZLogInformation($"Setting loaded from {ConfigPath} successfully");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(ConfigPath))
        {
            Logger.ZLogInformation($"Setting file not found, using default settings");
            return;
        }

        Config? savedConfig;
        try
        {
            var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            await using (stream.ConfigureAwait(false))
            {
                savedConfig = await JsonSerializationService.DeserializeConfigAsync(stream).ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            BackupCorruptedConfig(ex);
            return;
        }

        if (savedConfig is null)
        {
            Logger.ZLogError($"Saved setting is null, using default settings");
            return;
        }

        Config.CopyFrom(savedConfig);
        Logger.ZLogInformation($"Setting loaded from {ConfigPath} successfully");
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
    public required IAppLocalService AppLocalService { get; init; }

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IJsonSerializationService JsonSerializationService { get; init; }

    [UsedImplicitly]
    public required ILogger<AppSettingService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required ISteamPathDiscovery SteamDiscovery { get; init; }

    #endregion Injections
}