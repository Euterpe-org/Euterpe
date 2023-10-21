namespace MuseDashModToolsUI.Contracts;

public interface IInfoJsonService
{
    /// <summary>
    ///     Initialize the service, pass in the InfoJson object
    /// </summary>
    /// <param name="infoJson"></param>
    void Initialize(InfoJson infoJson);

    /// <summary>
    ///     Choose the chart folder
    /// </summary>
    /// <returns></returns>
    Task OnChooseChartFolderAsync();
}