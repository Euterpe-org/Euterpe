using MuseDashModToolsUI.Models;

namespace MuseDashModToolsUI.Contracts.ViewModels;

public interface IMainWindowViewModel
{
    List<TabView> Tabs { get; }
}