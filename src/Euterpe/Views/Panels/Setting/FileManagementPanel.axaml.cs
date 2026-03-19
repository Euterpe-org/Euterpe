namespace Euterpe.Views.Panels.Setting;

[Route("/setting/file", DisplayName = Panel_Setting_FileManagement, Order = 3)]
public sealed partial class FileManagementPanel : UserControl
{
    public FileManagementPanel()
    {
        InitializeComponent();
    }
}