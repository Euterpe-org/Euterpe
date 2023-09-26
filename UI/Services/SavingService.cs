using System.IO;
using System.IO.Abstractions;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class SavingService : ISavingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IPlatformService _platformService;
    private bool _isSavedLoaded;

    private static string ConfigFolderPath
    {
        get
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseDashModTools");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    private static string SettingPath => Path.Combine(ConfigFolderPath, "Settings.json");

    [UsedImplicitly]
    public IMessageBoxService MessageBoxService { get; init; }

    [UsedImplicitly]
    public ISerializeService SerializeService { get; init; }

    [UsedImplicitly]
    public Lazy<ILocalService> LocalService { get; init; }

    [UsedImplicitly]
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }

    [UsedImplicitly]
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    public SavingService(IFileSystem fileSystem, ILogger logger, IPlatformService platformService)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _platformService = platformService;
        Load().Wait();
    }

    public Setting Settings { get; } = new();

    public async Task InitializeSettings()
    {
        _logger.Information("Initializing settings...");

        if (!_isSavedLoaded)
        {
            _logger.Error("Settings.json not found or invalid, getting game path...");
            await TryGetGameFolderPath();
        }

        await CheckSettingValidity();

        // Update path in SettingsView and log content in LogAnalysisView
        SettingsViewModel.Value.UpdatePath();
        await LogAnalysisViewModel.Value.Initialize();
    }

    public async Task OnChoosePath(bool isInitializeTabs = false)
    {
        var path = await GetChosenPath();

        if (path == Settings.MuseDashFolder)
        {
            _logger.Information("Path not changed");
            return;
        }

        _logger.Information("User chose path {Path}", path);
        Settings.MuseDashFolder = path;

        await CheckSettingValidity();

        if (isInitializeTabs)
        {
            SettingsViewModel.Value.UpdatePath();
            await LogAnalysisViewModel.Value.Initialize();
            await ModManageViewModel.Value.Initialize();
        }
    }

    public async Task Save()
    {
        var json = SerializeService.SerializeSetting(Settings);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved to Settings.json");
    }
}