namespace Euterpe.Features.Setting;

public sealed class ExperiencePanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public required Config Config { get; init; }

    #endregion Injections
}