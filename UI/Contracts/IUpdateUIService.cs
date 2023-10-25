namespace MuseDashModToolsUI.Contracts;

public interface IUpdateUIService
{
    /// <summary>
    ///     Update Text when UI reloads
    /// </summary>
    void UpdateText();

    void ChangeTheme(string themeName);

    /// <summary>
    ///     Initialize All Tabs
    /// </summary>
    /// <returns></returns>
    Task InitializeAllTabsAsync();

    /// <summary>
    ///     Initialize ModManage and LogAnalysis tabs when path changed
    /// </summary>
    /// <returns></returns>
    Task InitializeTabsOnChoosePathAsync();
}