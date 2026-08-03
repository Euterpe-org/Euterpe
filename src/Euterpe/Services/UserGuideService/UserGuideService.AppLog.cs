using Coachlight.Avalonia.Building;
using Coachlight.Avalonia.Enums;
using Coachlight.Avalonia.Models;

namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    public const string AppLogTargetId = "AppLog";
    public const string LoggingOpenMenuTargetId = "LoggingOpenMenu";

    private Tour CreateAppLogTour() =>
        CreateTourBuilder("setup-app-log", true)
            .Modal(step => step
                .Title(XAML.Panel_Logging_AppLog)
                .Text(FormatTransition(XAML.Panel_Logging_AppLog))
                .OnEnter(() => NavigationService.NavigateTo("/logging/app")))
            .Coachmark(LoggingOpenMenuTargetId, step => step
                .Title(UserGuide_Title_LogFolders)
                .Text(UserGuide_Content_LogFolders)
                .Placement(Side.Bottom)
                .Interactive()
                .SkipIfMissing(false))
            .Coachmark(AppLogTargetId, step => step
                .Title(XAML.Panel_Logging_AppLog)
                .Text(UserGuide_Content_AppLog)
                .Interactive(false)
                .SkipIfMissing(false))
            .Build();
}
