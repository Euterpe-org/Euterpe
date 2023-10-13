#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace MuseDashModToolsUI.ViewModels.Tabs;

public class InfoJsonViewModel : ViewModelBase, IInfoJsonViewModel
{
    public IMessageBoxService MessageBoxService { get; init; }
}