namespace Euterpe.Services;

public sealed partial class DeepLinkService
{
    private async Task HandleEpkFileAsync(string filePath)
    {
        Logger.ZLogInformation($"Opening EPK file: {filePath}");

        await NavigationService.NavigateToAsync("/charting/toolkit").ConfigureAwait(true);
        await GameScope.Value.Resolve<CharterToolkitPanelViewModel>().OpenEpkAsync(filePath).ConfigureAwait(false);
    }
}
