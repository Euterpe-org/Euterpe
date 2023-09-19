using System.IO;
using System.IO.Abstractions;
using MuseDashModToolsUI.Contracts;
using MuseDashModToolsUI.Contracts.ViewModels;
using MuseDashModToolsUI.Models;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace MuseDashModToolsUI.Services;

public partial class SavingService : ISavingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

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
    public IMessageBoxService MessageBoxService { get; init; }
    public Lazy<ILocalService> LocalService { get; init; }
    public Lazy<ILogAnalysisViewModel> LogAnalysisViewModel { get; init; }
    public Lazy<IModManageViewModel> ModManageViewModel { get; init; }
    public Lazy<ISettingsViewModel> SettingsViewModel { get; init; }

    public SavingService(ILogger logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
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
            var settings = JsonConvert.DeserializeObject<Setting>(text)!;
            await NullSettingCatch(settings);

            Settings = settings.Clone();
            await LocalService.Value.CheckValidPath();
            LogAnalysisViewModel.Value.Initialize();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while initializing settings");
            await MessageBoxService.ErrorMessageBox(ex.ToString());
        }
    }

    public async Task Save()
    {
        var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
        await _fileSystem.File.WriteAllTextAsync(SettingPath, json);
        _logger.Information("Settings saved");
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
}