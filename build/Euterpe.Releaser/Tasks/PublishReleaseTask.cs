namespace Euterpe.Releaser.Tasks;

[TaskName("PublishRelease")]
public sealed class PublishReleaseTask(VelopackApiClient apiClient) : AsyncFrostingTask<ReleaseContext>
{
    public override Task RunAsync(ReleaseContext context)
    {
        context.EnsureGitHubActions();
        context.Information("Publishing staged Velopack version {0}", context.Version);
        return apiClient.PublishAsync(context.Version);
    }
}
