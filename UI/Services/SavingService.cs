using System.IO;
using System.IO.Abstractions;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class SavingService : ISavingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IPlatformService _platformService;

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

    public Setting Settings { get; private set; } = new();

    // TODO Fix recursive initialize setting (lxy, 2023/9/18) Planning Time: 2 months
    public async Task InitializeSettings()
    {
        _logger.Information("Initializing settings...");
        try
        {
            if (!_fileSystem.File.Exists(SettingPath))
            {
                _logger.Error("Settings.json not found, creating new one");
                await TryGetGameFolderPath();
            }

            var text = await _fileSystem.File.ReadAllTextAsync(SettingPath);
            var settings = SerializeService.DeserializeSetting(text)!;
            await NullSettingCatch(settings);

            Settings = settings.Clone();
            await LocalService.Value.CheckValidPath();
            LogAnalysisViewModel.Value.Initialize();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while initializing settings");
            await MessageBoxService.ErrorMessageBox(ex);
        }
    }

    public async Task OnChoosePath()
    {
        var path = await GetChosenPath();

        if (path == Settings.MuseDashFolder)
        {
            _logger.Information("Path not changed");
            return;
        }

        _logger.Information("User chose path {Path}", path);
        await WriteSettings(path!);

        ModManageViewModel.Value.Initialize();
        LogAnalysisViewModel.Value.Initialize();
    }

    public async Task Save()
    {
        var json = SerializeService.SerializeSetting(Settings);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved");
    }
}