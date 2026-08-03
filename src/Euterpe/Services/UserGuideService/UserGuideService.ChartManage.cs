using Coachlight.Avalonia.Building;
using Coachlight.Avalonia.Enums;
using Coachlight.Avalonia.Models;

namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    public const string ChartManageTargetId = "ChartManage";

    private Tour CreateChartManageTour() =>
        CreateTourBuilder("setup-chart-manage")
            .Modal(step => step
                .Title(XAML.Panel_Charting_ChartManage)
                .Text(FormatTransition(XAML.Panel_Charting_ChartManage))
                .OnEnter(() => NavigationService.NavigateTo("/charting/manage")))
            .Coachmark(ChartManageTargetId, step => step
                .Title(XAML.Panel_Charting_ChartManage)
                .Text(UserGuide_Content_ChartManage)
                .Placement(Side.Bottom)
                .Interactive(false)
                .SkipIfMissing(false))
            .Build();
}
