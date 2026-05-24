namespace Euterpe.Features.Charting;

public sealed class CharterToolkitPanelViewModel : ViewModelBase
{
    #region Injections

    public required IArchiveService ArchiveService { get; init; }
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}