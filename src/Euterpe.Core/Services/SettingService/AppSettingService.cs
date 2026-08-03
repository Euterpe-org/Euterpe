namespace Euterpe.Core;

internal sealed partial class AppSettingService : IAppSettingService
{
    private const string ConfigFileName = "Config.json";
    private static readonly string ConfigPath = Path.Combine(AppDataFolder, ConfigFileName);

    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            Logger.LogInformation("Setting file not found, using default settings");
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
            Logger.LogError("Saved setting is null, using default settings");
            return;
        }

        Config.CopyFrom(savedConfig);
        Logger.LogInformation("Setting loaded from {ConfigPath} successfully", ConfigPath);
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(ConfigPath))
        {
            Logger.LogInformation("Setting file not found, using default settings");
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
            Logger.LogError("Saved setting is null, using default settings");
            return;
        }

        Config.CopyFrom(savedConfig);
        Logger.LogInformation("Setting loaded from {ConfigPath} successfully", ConfigPath);
    }

    public void Save()
    {
        using var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        JsonSerializationService.SerializeConfig(stream, Config);
        Logger.LogInformation("Setting saved to {ConfigPath} successfully", ConfigPath);
    }

    public async Task SaveAsync()
    {
        var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        await using (stream.ConfigureAwait(false))
        {
            await JsonSerializationService.SerializeConfigAsync(stream, Config).ConfigureAwait(false);
            Logger.LogInformation("Setting saved to {ConfigPath} successfully", ConfigPath);
        }
    }

    public async Task ValidateSteamAsync()
    {
        Logger.LogInformation("Checking for valid Steam setting");

        await CheckSteamFolderAsync().ConfigureAwait(true);
        await CheckSteamExecPathAsync().ConfigureAwait(true);

        Logger.LogInformation("Steam setting validated");
    }

    #region Injections

    public required IAppLocalService AppLocalService { get; init; }
    public required Config Config { get; init; }
    public required IJsonSerializationService JsonSerializationService { get; init; }
    public required ILogger<AppSettingService> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required ISteamPathDiscovery SteamDiscovery { get; init; }

    #endregion Injections
}
