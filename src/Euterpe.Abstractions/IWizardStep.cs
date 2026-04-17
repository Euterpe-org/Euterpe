namespace Euterpe.Abstractions;

public interface IWizardStep
{
    WizardTaskKind Kind { get; }

    Task ExecuteAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}