namespace MuseDashModToolsUI.Contracts;

public interface ISavingService
{
    /// <summary>
    ///     Load saved setting from Settings.json
    ///     Create config and chart folder if not exist
    ///     Delete Updater
    /// </summary>
    void LoadSettings();

    /// <summary>
    ///     Initialize Settings
    /// </summary>
    /// <returns></returns>
    Task InitializeSettingsAsync();

    /// <summary>
    ///     Get muse dash folder path and Initialize tabs
    /// </summary>
    /// <returns>Is path changed</returns>
    Task<bool> OnChooseGamePathAsync();

    /// <summary>
    ///     Save Settings into Settings.json
    /// </summary>
    /// <returns></returns>
    Task SaveAsync();
}