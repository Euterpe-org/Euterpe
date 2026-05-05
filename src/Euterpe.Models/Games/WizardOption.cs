namespace Euterpe.Models.Games;

public sealed partial class WizardOption(WizardOptionKinds kinds, LocalizedString displayName, LocalizedString description) : ObservableObject
{
    public WizardOptionKinds Kinds { get; } = kinds;

    public LocalizedString DisplayName { get; } = displayName;

    public LocalizedString Description { get; } = description;

    public bool IsRequired { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}