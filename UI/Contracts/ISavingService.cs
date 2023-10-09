namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    public Setting Settings { get; }
    public string ModLinksPath { get; }
    public string ChartFolderPath { get; }

    /// <summary>
    ///     Initialize Settings
    /// </summary>
    /// <returns></returns>
    Task InitializeSettings();

    /// <summary>
    ///     Get muse dash folder path and Initialize tabs
    /// </summary>
    /// <returns></returns>
    Task OnChoosePath();

    /// <summary>
    ///     Save Settings into Settings.json
    /// </summary>
    /// <returns></returns>
    Task Save();
}