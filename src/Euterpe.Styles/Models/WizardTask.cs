namespace Euterpe.Styles.Models;

public sealed partial class WizardTask(string name, LocalizedString displayName, LocalizedString description) : ObservableObject
{
    public string Name { get; } = name;

    public LocalizedString DisplayName { get; } = displayName;

    public LocalizedString Description { get; } = description;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}