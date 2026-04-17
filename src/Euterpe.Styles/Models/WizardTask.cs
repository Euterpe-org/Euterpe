using Euterpe.Models.Enums;

namespace Euterpe.Styles.Models;

public sealed partial class WizardTask(WizardTaskKind kind, LocalizedString displayName, LocalizedString description) : ObservableObject
{
    public WizardTaskKind Kind { get; } = kind;

    public LocalizedString DisplayName { get; } = displayName;

    public LocalizedString Description { get; } = description;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}