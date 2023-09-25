namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface IModManageViewModel
{
    /// <summary>
    ///     Initialize mod list and start dll monitor
    /// </summary>
    /// <returns></returns>
    Task Initialize();
}