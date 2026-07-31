namespace Euterpe.Releaser;

internal sealed class ReleaseProcessRunner
{
    private readonly Logger _logger = LogManager.GetLogger(nameof(ReleaseProcessRunner));

    public Task RunVpkAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken) =>
        RunDotNetAsync(["tool", "run", "vpk", "--", .. arguments], cancellationToken);

    public async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        await Cli.Wrap("dotnet")
            .WithArguments(arguments)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => _logger.Info($"{line}")))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => _logger.Error($"{line}")))
            .ExecuteAsync(cancellationToken);
    }
}
