namespace Euterpe.Releaser;

internal sealed class ReleaseCommands
{
    private readonly Logger _logger = LogManager.GetLogger(nameof(ReleaseCommands));

    [Command("stage")]
    public Task StageAsync(
        [FromServices] RidReleaseStager stager,
        string rid,
        CancellationToken cancellationToken = default) =>
        stager.StageAsync(ReleaseRuntime.Parse(rid), ReleaseVersion, cancellationToken);

    [Command("publish")]
    public async Task PublishAsync(
        CancellationToken cancellationToken = default)
    {
        using var apiClient = new VelopackApiClient();

        _logger.Info($"Publishing staged Velopack version {ReleaseVersion.ToString()}");
        await apiClient.PublishAsync(ReleaseVersion, cancellationToken);
    }
}
