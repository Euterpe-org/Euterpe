namespace Euterpe.Features.Setting;

[Route("/setting/file", DisplayName = Panel_Setting_FileManagement, Order = 3)]
[PerGameView]
public sealed partial class FileManagementPanel : UserControl
{
    public FileManagementPanel()
    {
        InitializeComponent();
    }
}