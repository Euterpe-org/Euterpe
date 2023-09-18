using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    public Setting Settings { get; }

    /// <summary>
    ///     Initialize Settings
    /// </summary>
    Task InitializeSettings();

    /// <summary>
    ///     Save Settings into Settings.json
    /// </summary>
    Task Save();

    /// <summary>
    ///     GetPath and write to Settings.json
    ///     Initialize tabs
    /// </summary>
    Task OnChoosePath();
}