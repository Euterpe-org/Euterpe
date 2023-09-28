namespace MuseDashModToolsUI.Contracts;

public interface IUpdateUIService
{
    /// <summary>
    ///     Update Text when UI reloads
    /// </summary>
    void UpdateText();

    /// <summary>
    ///     Initialize All Tabs
    /// </summary>
    /// <returns></returns>
    Task InitializeAllTabs();

    /// <summary>
    ///     Initialize ModManage and LogAnalysis tabs when path changed
    /// </summary>
    /// <returns></returns>
    Task InitializeTabsOnChoosePath();
}