namespace Euterpe.Abstractions;

public interface IWizardStep
{
    string Name { get; }

    Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}