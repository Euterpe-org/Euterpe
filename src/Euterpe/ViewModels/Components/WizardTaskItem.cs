namespace Euterpe.ViewModels.Components;

public sealed partial class WizardTaskItem(string name) : ObservableObject
{
    public string Name { get; } = name;

    [ObservableProperty]
    public partial WizardTaskStatus Status { get; set; }
}
