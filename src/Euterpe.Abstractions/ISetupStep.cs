namespace Euterpe.Abstractions;

public interface ISetupStep
{
    SetupOptionKinds Kinds { get; }

    Task ExecuteAsync(IProgress<string> progress, CancellationToken cancellationToken = default);
}
