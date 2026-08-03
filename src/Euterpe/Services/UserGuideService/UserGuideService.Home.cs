using Coachlight.Avalonia.Building;
using Coachlight.Avalonia.Enums;
using Coachlight.Avalonia.Models;

namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    public const string MainNavigationTargetId = "MainNavigation";
    public const string PlayButtonTargetId = "PlayButton";

    private static Tour CreateHomeTour() =>
        CreateTourBuilder("setup-home")
            .Modal(step => step
                .Title(UserGuide_Title_Welcome)
                .Text(UserGuide_Content_Welcome))
            .Coachmark(MainNavigationTargetId, step => step
                .Title(UserGuide_Title_MainNavigation)
                .Text(UserGuide_Content_MainNavigation)
                .Placement(Side.Right)
                .Interactive(false))
            .Coachmark(PlayButtonTargetId, step => step
                .Title(UserGuide_Title_Launch)
                .Text(UserGuide_Content_Launch)
                .Placement(Side.Left)
                .Interactive(false))
            .Build();
}
