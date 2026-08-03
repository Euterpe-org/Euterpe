using Coachlight.Avalonia.Building;
using Coachlight.Avalonia.Enums;
using Coachlight.Avalonia.Models;

namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    public const string ModManageTargetId = "ModManage";

    private Tour CreateModManageTour() =>
        CreateTourBuilder("setup-mod-manage")
            .Modal(step => step
                .Title(XAML.Panel_Modding_ModManage)
                .Text(FormatTransition(XAML.Panel_Modding_ModManage))
                .OnEnter(() => NavigationService.NavigateTo("/modding/manage")))
            .Coachmark(ModManageTargetId, step => step
                .Title(XAML.Panel_Modding_ModManage)
                .Text(UserGuide_Content_ModManage)
                .Placement(Side.Right)
                .Interactive(false)
                .SkipIfMissing(false))
            .Build();
}
