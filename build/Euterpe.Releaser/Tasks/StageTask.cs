namespace Euterpe.Releaser.Tasks;

[TaskName("Stage")]
[IsDependentOn(typeof(FetchReleaseBasesTask))]
[IsDependentOn(typeof(PackTask))]
public sealed class StageTask(VelopackApiClient apiClient) : AsyncFrostingTask<ReleaseContext>
{
    public override bool ShouldRun(ReleaseContext context) => !context.IsVersionPublished;

    public override async Task RunAsync(ReleaseContext context)
    {
        foreach (var channel in context.PackageChannels)
        {
            var outputDirectory = context.GetPackageDirectory(channel);
            List<(string Type, string FileName)> assets =
            [
                ("full", context.GetFullPackageFileName(channel))
            ];
            if (context.ReleaseBases[channel] is not null)
            {
                assets.Add(("delta", context.GetDeltaPackageFileName(channel)));
            }

            assets.Add(("installer", context.GetInstallerFileName(channel)));

            foreach (var asset in assets)
            {
                var assetPath = outputDirectory.CombineWithFilePath(asset.FileName);
                context.Information("Staging {0} {1}: {2}", channel, asset.Type, assetPath);
                await apiClient.UploadAssetAsync(channel, context.Version, asset.Type, assetPath.FullPath);
            }
        }
    }
}
