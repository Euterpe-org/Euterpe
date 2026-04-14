namespace Euterpe.Views.Panels.Modding;

[Route("/modding/manage", DisplayName = Panel_Modding_ModManage, Order = 0)]
public sealed partial class ModManagePanel : UserControl
{
    public ModManagePanel()
    {
        InitializeComponent();

        var scroller = this.FindControl<ScrollViewer>("ScreenshotsScroller");
        if (scroller is not null)
        {
            scroller.PointerWheelChanged += (_, e) =>
            {
                scroller.Offset = scroller.Offset.WithX(scroller.Offset.X - e.Delta.Y * 60);
                e.Handled = true;
            };
        }
    }
}