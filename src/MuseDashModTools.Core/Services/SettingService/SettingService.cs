namespace MuseDashModTools.Core;

internal sealed partial class SettingService : ISettingService
{
    private const string ConfigFileName = "Config.json";
    private static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(MuseDashModTools));
    private static readonly string ConfigPath = Path.Combine(ConfigFolder, ConfigFileName);

    public async Task LoadAsync()
    {
        Directory.CreateDirectory(ConfigFolder);
        if (File.Exists(ConfigPath))
        {
            await using var stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
            var savedConfig = await JsonSerializationService.DeserializeConfigAsync(stream).ConfigureAwait(true);
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

    public async Task SaveAsync()
    {
        await using var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        await JsonSerializationService.SerializeConfigAsync(stream, Config).ConfigureAwait(false);
        Logger.ZLogInformation($"Setting saved to {ConfigPath} successfully");
    }

    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    [UsedImplicitly]
    public required IJsonSerializationService JsonSerializationService { get; init; }

    [UsedImplicitly]
    public required ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public required ILogger<SettingService> Logger { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public required IPlatformService PlatformService { get; init; }

    #endregion Injections
}