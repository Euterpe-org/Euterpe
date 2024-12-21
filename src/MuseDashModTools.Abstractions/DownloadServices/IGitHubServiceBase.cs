namespace MuseDashModTools.Abstractions;

public interface IGitHubServiceBase
{
    Task DownloadReleaseByTagAsync(string tag, CancellationToken cancellationToken = default);
}