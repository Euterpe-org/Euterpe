namespace MuseDashModToolsUI.Contracts;

public interface IUpdateUIService
{
    /// <summary>
    ///     Update Text when UI reloads
    /// </summary>
    void UpdateText();

    /// <summary>
    ///     Initialize Tabs
    /// </summary>
    /// <returns></returns>
    Task InitializeTabs();
}