namespace Euterpe.Features.Charting;

public sealed class CharterToolkitPanelViewModel : ViewModelBase
{
    #region Injections

    [UsedImplicitly]
    public required IArchiveService ArchiveService { get; init; }

    [UsedImplicitly]
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}