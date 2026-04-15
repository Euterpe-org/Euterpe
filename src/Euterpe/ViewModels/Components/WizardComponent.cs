namespace Euterpe.ViewModels.Components;

public sealed partial class WizardComponent(string name, string displayName) : ObservableObject
{
    public string Name { get; } = name;

    public string DisplayName { get; } = displayName;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
