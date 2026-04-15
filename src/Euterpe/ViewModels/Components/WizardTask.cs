namespace Euterpe.ViewModels.Components;

public sealed partial class WizardTask(string name, string displayName, string description) : ObservableObject
{
    public string Name { get; } = name;

    public string DisplayName { get; } = displayName;

    public string Description { get; } = description;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
