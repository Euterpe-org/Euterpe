namespace Euterpe.Abstractions;

public interface IWizardStep
{
    WizardOptionKinds Kinds { get; }

    Task ExecuteAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}