namespace Euterpe.Releaser;

internal sealed class ReleaseCommands
{
    [Command("stage")]
    public Task StageAsync(
        [FromServices] RidReleaseStager stager,
        string rid,
        CancellationToken cancellationToken = default) =>
        stager.StageAsync(ReleaseRuntime.Parse(rid), ReleaseVersion, cancellationToken);

    [Command("publish")]
    public async Task PublishAsync(
        [FromServices] ILogger<ReleaseCommands> logger,
        CancellationToken cancellationToken = default)
    {
        using var apiClient = new VelopackApiClient();

        logger.ZLogInformation($"Publishing staged Velopack version {ReleaseVersion}");
        await apiClient.PublishAsync(ReleaseVersion, cancellationToken);
    }
}
