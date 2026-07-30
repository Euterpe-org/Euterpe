namespace Euterpe.Releaser;

internal sealed class ReleaseProcessRunner(ILogger<ReleaseProcessRunner> logger)
{
    public Task RunVpkAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken) =>
        RunDotNetAsync(["tool", "run", "vpk", "--", .. arguments], cancellationToken);

    public async Task RunDotNetAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        await Cli.Wrap("dotnet")
            .WithArguments(arguments)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(line => logger.ZLogInformation($"{line}")))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line => logger.ZLogError($"{line}")))
            .ExecuteAsync(cancellationToken);
    }
}
