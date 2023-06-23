using Avalonia.Controls;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class ModManageView : UserControl
{
    public ModManageView()
    {
        InitializeComponent();
        DrawerList.SelectedIndex = 0;
    }
}