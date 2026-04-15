namespace Euterpe.ViewModels.Components;

public sealed partial class WizardIdentityOption(WizardIdentity identity, string iconKey, string title, string description) : ObservableObject
{
    public WizardIdentity Identity { get; } = identity;

    public string IconKey { get; } = iconKey;

    public string Title { get; } = title;

    public string Description { get; } = description;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
