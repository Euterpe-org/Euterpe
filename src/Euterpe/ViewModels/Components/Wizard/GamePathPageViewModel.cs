namespace Euterpe.ViewModels.Components.Wizard;

public sealed partial class GamePathPageViewModel : WizardPageViewModelBase
{
    public override LocalizedString Title => Wizard_Title_GamePath;

    public override bool CanGoBack => false;

    public override bool CanGoNext => IsSelectedFolderValid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedFolderValid))]
    [NotifyPropertyChangedFor(nameof(ShowInvalidMessage))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    public partial string? SelectedFolder { get; set; }

    public bool IsSelectedFolderValid =>
        !SelectedFolder.IsNullOrEmpty() && GamePaths.CheckIsValidGameFolder(SelectedFolder);

    public bool ShowInvalidMessage => !SelectedFolder.IsNullOrEmpty() && !IsSelectedFolderValid;

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        if (GamePaths.TryGetGameFolder(out var detected))
        {
            SelectedFolder = detected;
        }

        Logger.ZLogInformation($"{nameof(GamePathPageViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        var folder = await FileSystemPickerService.GetSingleFolderPathAsync(FolderDialog_Title_ChooseMuseDashFolder).ConfigureAwait(true);
        if (folder.IsNullOrEmpty())
        {
            return;
        }

        SelectedFolder = folder;
        Logger.ZLogInformation($"User selected {GameConfig.DisplayName} folder: {folder}");
    }

    partial void OnSelectedFolderChanged(string? value) => GameConfig.Folder = value;

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required IGamePathDiscovery GamePaths { get; init; }

    [UsedImplicitly]
    public required IFileSystemPickerService FileSystemPickerService { get; init; }

    [UsedImplicitly]
    public required ILogger<GamePathPageViewModel> Logger { get; init; }

    #endregion Injections
}