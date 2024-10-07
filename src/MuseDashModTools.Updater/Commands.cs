using Cocona;

namespace MuseDashModTools.Updater;

public sealed class Commands(ILocalService localService)
{
    private readonly ILocalService _localService = localService;


    [Command("extract")]
    public bool ExtractZipFile(
        [Argument(Description = "The path of the zip file")] string zipPath,
        [Argument(Description = "The extract folder path")] string extractPath)
        => _localService.ExtractZipFile(zipPath, extractPath);
}