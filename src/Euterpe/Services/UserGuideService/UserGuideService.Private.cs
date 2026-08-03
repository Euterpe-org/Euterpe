using Coachlight.Avalonia;
using Coachlight.Avalonia.Building;
using Coachlight.Avalonia.Controller;
using Coachlight.Avalonia.Models;

namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    private IReadOnlyList<Tour> CreateSetupTours() =>
    [
        CreateHomeTour(),
        CreateModManageTour(),
        CreateChartManageTour(),
        CreateAppLogTour()
    ];

    private static TourBuilder CreateTourBuilder(string id, bool isFinal = false) =>
        TourBuilder.Create(id)
            .Labels(new TourLabels
            {
                Back = XAML.Button_Back,
                Done = isFinal ? XAML.Button_Done : XAML.Button_Next,
                Next = XAML.Button_Next,
                Skip = XAML.Button_Skip
            });

    private static string FormatTransition(string destination) =>
        string.Format(CultureInfo.CurrentCulture, UserGuide_Content_Transition, destination);

    private static void StartTour(
        IReadOnlyList<Tour> tours,
        int index,
        TaskCompletionSource completion)
    {
        var controller = GetCurrentMainWindow().StartTour(tours[index]);
        controller.Ended += OnEnded;

        void OnEnded(object? sender, TourEndReason reason)
        {
            controller.Ended -= OnEnded;
            if (reason is TourEndReason.Completed && index + 1 < tours.Count)
            {
                StartTour(tours, index + 1, completion);
                return;
            }

            completion.TrySetResult();
        }
    }
}
