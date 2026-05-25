namespace Euterpe.Features.Charting;

[Route("/charting/toolkit", DisplayName = Panel_Charting_CharterToolkit, Order = 1)]
public sealed class CharterToolkitPanelViewModel : ViewModelBase
{
    #region Injections

    public required IArchiveService ArchiveService { get; init; }
    public required ILogger<CharterToolkitPanelViewModel> Logger { get; init; }
    public required IFileSystemPickerService FileSystemPickerService { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }

    #endregion Injections
}