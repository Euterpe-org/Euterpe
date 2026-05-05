using Avalonia.Input;

namespace Euterpe.Views.Panels.Modding;

[Route("/modding/manage", DisplayName = Panel_Modding_ModManage, Order = 0)]
[PerGameView]
public sealed partial class ModManagePanel : UserControl
{
    private const double WheelScrollStep = 60d;

    public ModManagePanel()
    {
        InitializeComponent();

        ScreenshotsScroller.PointerWheelChanged += OnScreenshotsWheelChanged;
    }

    private void OnScreenshotsWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var maxX = ScreenshotsScroller.Extent.Width - ScreenshotsScroller.Viewport.Width;
        if (maxX <= 0)
        {
            return;
        }

        var currentX = ScreenshotsScroller.Offset.X;
        var targetX = Math.Clamp(currentX - e.Delta.Y * WheelScrollStep, 0, maxX);
        if (Math.Abs(targetX - currentX) < double.Epsilon)
        {
            return;
        }

        ScreenshotsScroller.Offset = ScreenshotsScroller.Offset.WithX(targetX);
        e.Handled = true;
    }
}