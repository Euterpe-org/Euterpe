namespace Euterpe.Releaser.Tasks;

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<ReleaseContext>
{
    public override void Run(ReleaseContext context)
    {
        context.CleanDirectory(context.ApplicationDirectory);
        context.CleanDirectory(context.GitHubReleaseDirectory);
        context.CleanDirectories(context.BaseChannels.Select(context.GetPackageDirectory));
    }
}
