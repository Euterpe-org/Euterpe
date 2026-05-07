namespace Euterpe.ViewModels.Components.Setup;

public sealed partial class GamePathPageViewModel : SetupPageViewModelBase
{
    public override LocalizedString Title => Wizard_Title_GamePath;

    public override bool CanGoBack => false;

    public override bool CanGoNext => IsSelectedFolderValid;

    [ObservableProperty]
    public partial string? SelectedFolder { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowInvalidMessage))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    public partial bool IsSelectedFolderValid { get; set; }

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

    partial void OnSelectedFolderChanged(string? value)
    {
        GameConfig.Folder = value;
        IsSelectedFolderValid = GamePaths.CheckIsValidGameFolder(value);
    }

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