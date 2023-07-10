using Avalonia.Controls;

namespace MuseDashModToolsUI.Views.Tabs;

public partial class ModManage : UserControl
{
    public ModManage()
    {
        InitializeComponent();
        DrawerList.SelectedIndex = 0;
    }
}