namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    public Setting Settings { get; }

    /// <summary>
    ///     Initialize Settings
    /// </summary>
    /// <returns></returns>
    Task InitializeSettings();

    /// <summary>
    ///     Get muse dash folder path and Initialize tabs
    /// </summary>
    /// <param name="isInitializeTabs"></param>
    /// <returns></returns>
    Task OnChoosePath(bool isInitializeTabs = false);

    /// <summary>
    ///     Save Settings into Settings.json
    /// </summary>
    /// <returns></returns>
    Task Save();
}