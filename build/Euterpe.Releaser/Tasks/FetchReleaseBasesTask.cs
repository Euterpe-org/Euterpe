namespace Euterpe.Releaser.Tasks;

[TaskName("FetchReleaseBases")]
[IsDependentOn(typeof(CleanTask))]
public sealed class FetchReleaseBasesTask(VelopackApiClient apiClient) : AsyncFrostingTask<ReleaseContext>
{
    public override async Task RunAsync(ReleaseContext context)
    {
        context.EnsureGitHubActions();

        foreach (var channel in context.BaseChannels)
        {
            context.ReleaseBases.Add(channel, await apiClient.GetReleaseBaseAsync(channel));
        }

        if (context.IsVersionPublished)
        {
            context.Information("Velopack version {0} is already published for {1}; skipping staging", context.Version, context.Rid);
            return;
        }

        foreach (var channel in context.PackageChannels)
        {
            if (context.ReleaseBases[channel] is not { } releaseBase)
            {
                continue;
            }

            context.Information("Downloading {0} base {1}", channel, releaseBase.Version);
            var destinationPath = context.GetPackageDirectory(channel).CombineWithFilePath(context.GetFullPackageFileName(channel, releaseBase.Version));
            await apiClient.DownloadReleaseBaseAsync(releaseBase.DownloadPath, destinationPath.FullPath);
        }
    }
}
