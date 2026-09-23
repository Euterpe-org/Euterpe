using Cake.Common.Tools.DotNet;
using Cake.Core.IO;

namespace Euterpe.Releaser.Tasks;

[TaskName("Pack")]
[IsDependentOn(typeof(PublishAppTask))]
public sealed class PackTask : FrostingTask<ReleaseContext>
{
    public override bool ShouldRun(ReleaseContext context) => !context.IsVersionPublished;

    public override void Run(ReleaseContext context)
    {
        context.DotNetTool(null, "tool", "restore");

        foreach (var channel in context.PackageChannels)
        {
            context.Information("Packing {0}", channel);
            context.DotNetTool(null, "vpk", ProcessArgumentBuilder.FromStringsQuoted(
            [
                "pack",
                "--packId", PackageId,
                "--packVersion", context.Version.ToString(),
                "--packDir", context.ApplicationDirectory.FullPath,
                "--runtime", context.Rid,
                "--channel", channel,
                "--delta", "BestSpeed",
                "--outputDir", context.GetPackageDirectory(channel).FullPath,
                .. context.PlatformVpkArguments
            ]));
        }

        var primaryChannel = context.PrimaryChannel;
        var installerPath = context.GetPackageDirectory(primaryChannel).CombineWithFilePath(context.GetInstallerFileName(primaryChannel));
        var gitHubInstallerPath = context.GitHubReleaseDirectory.CombineWithFilePath(context.GitHubInstallerFileName);
        context.CopyFile(installerPath, gitHubInstallerPath);
        context.Information("Exported GitHub Release installer to {0}", gitHubInstallerPath);
    }
}
