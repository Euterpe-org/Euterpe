using Avalonia.Controls;
using MuseDashModToolsUI.Contracts.ViewModels;
using Splat;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class ModManageView : UserControl
{
    public ModManageView()
    {
        DataContext = Locator.Current.GetRequiredService<IModManageViewModel>();
        InitializeComponent();
        DrawerList.SelectedIndex = 0;
    }
}