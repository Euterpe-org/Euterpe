using System.IO.Abstractions;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public sealed partial class SavingService : ISavingService
{
    private bool _isSavedLoaded;

    private static string ConfigFolderPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");

    private static string SettingPath => Path.Combine(ConfigFolderPath, "Settings.json");

    [UsedImplicitly]
    public Setting Settings { get; init; }

    public string ModLinksPath => Path.Combine(ConfigFolderPath, "ModLinks.json");
    public string ChartFolderPath => Path.Combine(ConfigFolderPath, "Charts");

    public async Task InitializeSettingsAsync()
    {
        Logger.Information("Initializing settings...");

        if (!_isSavedLoaded)
        {
            Logger.Warning("Didn't load setting from Settings.json, getting game path...");
            if (!await TryGetGameFolderPath())
            {
                await MessageBoxService.WarningMessageBox(MsgBox_Content_ChoosePath);
                await OnChooseGamePathAsync();
            }
        }

        await CheckSettingValidity();
        UpdateUIService.ChangeTheme(Settings.Theme);
        await UpdateUIService.InitializeAllTabsAsync();

        Logger.Information("Settings initialize finished");
    }

    public void LoadSettings()
    {
        LoadSavedSetting();
        CreateSavingFolders();
        DeleteUpdater();
    }

    public async Task SaveAsync()
    {
        var json = SerializationService.SerializeSetting(Settings);
        await FileSystem.File.WriteAllTextAsync(SettingPath, json);
        Logger.Information("Settings saved to Settings.json");
    }

    public async Task<bool> OnChooseGamePathAsync()
    {
        var path = await GetChosenPath();
        if (!await CheckValidPath(path))
        {
            return false;
        }

        Logger.Information("User chose path {Path}", path);
        Settings.MuseDashFolder = path;

        await CheckSettingValidity();
        return true;
    }

    #region Services

    [UsedImplicitly]
    public IFileSystem FileSystem { get; init; }

    [UsedImplicitly]
    public IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public ILocalService LocalService { get; init; }

    [UsedImplicitly]
    public ILogger Logger { get; init; }

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public IPlatformService PlatformService { get; init; }

    [UsedImplicitly]
    public ISerializationService SerializationService { get; init; }

    [UsedImplicitly]
    public IUpdateUIService UpdateUIService { get; init; }

    #endregion
}