namespace Euterpe.ViewModels.Components;

public sealed partial class WizardIdentityOption(WizardIdentity identity, string iconKey, string title, string description, string hoverColor) : ObservableObject
{
    public WizardIdentity Identity { get; } = identity;

    public string IconKey { get; } = iconKey;

    public string Title { get; } = title;

    public string Description { get; } = description;

    public string HoverColor { get; } = hoverColor;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
