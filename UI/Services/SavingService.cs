using System.IO.Abstractions;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public sealed partial class SavingService : ISavingService
{
    private const string SettingFile = "Settings.cfg";
    private bool _isSavedLoaded;

    private static string SettingPath =>
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name, SettingFile);

    public async Task InitializeSettingsAsync()
    {
        Logger.Information("Initializing settings...");

        if (!_isSavedLoaded)
        {
            Logger.Warning("Didn't load setting from {SettingFile}, getting game path...", SettingFile);
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
        await using var stream = new FileStream(SettingPath, FileMode.OpenOrCreate, FileAccess.Write);
        await SerializationService.SerializeSettingAsync(stream, Settings);
        Logger.Information("Settings saved to {SettingFile}", SettingFile);
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

    [UsedImplicitly]
    public Setting Settings { get; init; }

    #endregion
}