namespace Euterpe.Abstractions;

public interface IWizardStep
{
    WizardOptionKinds Kinds { get; }

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}