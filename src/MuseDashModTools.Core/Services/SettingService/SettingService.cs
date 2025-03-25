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

    public async Task SaveAsync()
    {
        throw new Exception("Test Exception");
        await using var stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write);
        await JsonSerializationService.SerializeConfigAsync(stream, Config).ConfigureAwait(false);
        Logger.ZLogInformation($"Setting saved to {ConfigPath} successfully");
    }

    public async Task ValidateAsync()
    {
        Logger.ZLogInformation($"Checking for valid setting...");
        await CheckMuseDashFolderAsync().ConfigureAwait(true);

        Logger.ZLogInformation($"Checking for valid setting done");
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