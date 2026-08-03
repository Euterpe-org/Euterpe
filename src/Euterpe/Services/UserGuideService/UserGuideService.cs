namespace Euterpe.Services;

public sealed partial class UserGuideService
{
    public Task ShowSetupGuideAsync()
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        StartTour(CreateSetupTours(), 0, completion);
        return completion.Task;
    }

    #region Injections

    public required NavigationService NavigationService { get; init; }

    #endregion Injections
}
