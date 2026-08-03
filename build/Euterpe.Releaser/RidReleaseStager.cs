using static Euterpe.Releaser.ReleaseArtifactNames;
using static Euterpe.Releaser.ReleasePlanner;

namespace Euterpe.Releaser;

internal sealed class RidReleaseStager(
    ReleaseProcessRunner processRunner,
    VelopackApiClient apiClient)
{
    private readonly Logger _logger = LogManager.GetLogger(nameof(RidReleaseStager));

    public async Task StageAsync(
        ReleaseRuntime runtime,
        SemVersion version,
        CancellationToken cancellationToken)
    {
        var context = new StageContext(Environment.CurrentDirectory, runtime, version, apiClient);
        var releaseBases = await GetReleaseBasesAsync(context, cancellationToken);
        var packageChannels = GetPackageChannels(runtime, version, releaseBases[runtime.BetaChannel] is not null);

        if (packageChannels.All(channel => releaseBases[channel]?.Version == version.ToString()))
        {
            _logger.Info($"Velopack version {version.ToString()} is already published for {runtime.Rid}; skipping staging");
            return;
        }

        var applicationDirectory = Path.Combine(context.RepositoryRoot, "artifacts", "output");
        await PublishApplicationAsync(context, cancellationToken);

        foreach (var channel in packageChannels)
        {
            await StageChannelAsync(
                context,
                channel,
                releaseBases[channel],
                applicationDirectory,
                cancellationToken);
        }

        ExportGitHubInstaller(context);
    }

    private async Task StageChannelAsync(
        StageContext context,
        string channel,
        VelopackReleaseBase? releaseBase,
        string applicationDirectory,
        CancellationToken cancellationToken)
    {
        var outputDirectory = Path.Combine(
            context.RepositoryRoot,
            "artifacts",
            "releases",
            channel);
        Directory.CreateDirectory(outputDirectory);

        if (releaseBase is not null)
        {
            _logger.Info($"Downloading {channel} base {releaseBase.Version}");
            var baseVersion = SemVersion.Parse(releaseBase.Version, SemVersionStyles.Strict);
            var destinationPath = Path.Combine(outputDirectory, GetFullPackageFileName(baseVersion, channel));
            await context.ApiClient.DownloadReleaseBaseAsync(releaseBase.DownloadPath, destinationPath, cancellationToken);
        }

        await PackChannelAsync(context, channel, applicationDirectory, outputDirectory, cancellationToken);

        List<(string Type, string Path)> assets =
        [
            ("full", Path.Combine(outputDirectory, GetFullPackageFileName(context.Version, channel)))
        ];
        if (releaseBase is not null)
        {
            assets.Add(("delta", Path.Combine(outputDirectory, GetDeltaPackageFileName(context.Version, channel))));
        }

        assets.Add(("installer", Path.Combine(outputDirectory, GetInstallerFileName(context.Runtime, channel))));

        foreach (var asset in assets)
        {
            _logger.Info($"Staging {channel} {asset.Type}: {asset.Path}");
            await context.ApiClient.UploadAssetAsync(
                channel,
                context.Version,
                asset.Type,
                asset.Path,
                cancellationToken);
        }
    }

    private static async Task<Dictionary<string, VelopackReleaseBase?>> GetReleaseBasesAsync(
        StageContext context,
        CancellationToken cancellationToken)
    {
        var releaseBases = new Dictionary<string, VelopackReleaseBase?>();
        foreach (var channel in GetBaseChannels(context.Runtime, context.Version))
        {
            var releaseBase = await context.ApiClient.GetReleaseBaseAsync(channel, cancellationToken);
            releaseBases.Add(channel, releaseBase);
        }

        return releaseBases;
    }

    private async Task PublishApplicationAsync(StageContext context, CancellationToken cancellationToken)
    {
        _logger.Info($"Publishing {context.Runtime.Rid} application files to artifacts/output");
        await processRunner.RunDotNetAsync(
            [
                "publish",
                ApplicationProject,
                "-c",
                "Release",
                "-r",
                context.Runtime.Rid
            ],
            cancellationToken);
    }

    private void ExportGitHubInstaller(StageContext context)
    {
        var channel = GetPrimaryChannel(context.Runtime, context.Version);
        var sourcePath = Path.Combine(
            context.RepositoryRoot,
            "artifacts",
            "releases",
            channel,
            GetInstallerFileName(context.Runtime, channel));
        var outputDirectory = Path.Combine(context.RepositoryRoot, "artifacts", "github-release");
        var destinationPath = Path.Combine(outputDirectory, GetGitHubInstallerFileName(context.Runtime));

        Directory.CreateDirectory(outputDirectory);
        File.Copy(sourcePath, destinationPath, true);
        _logger.Info($"Exported GitHub Release installer to {destinationPath}");
    }

    private async Task PackChannelAsync(
        StageContext context,
        string channel,
        string applicationDirectory,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        _logger.Info($"Packing {channel}");
        await processRunner.RunVpkAsync(
            [
                "pack",
                "--packId", PackageId,
                "--packVersion", context.Version.ToString(),
                "--packDir", applicationDirectory,
                "--mainExe", context.Runtime.MainExecutable,
                "--runtime", context.Runtime.Rid,
                "--channel", channel,
                "--delta", "BestSpeed",
                "--outputDir", outputDirectory,
                .. context.Runtime.ExtraVpkArguments
            ],
            cancellationToken);
    }

    private sealed record StageContext(
        string RepositoryRoot,
        ReleaseRuntime Runtime,
        SemVersion Version,
        VelopackApiClient ApiClient);
}
