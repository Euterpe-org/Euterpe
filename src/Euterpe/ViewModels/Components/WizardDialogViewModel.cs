using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.ViewModels.Components;

public sealed partial class WizardDialogViewModel : ViewModelBase, IDialogContext
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(IsLastPage))]
    public partial int CurrentPageIndex { get; set; }

    public IReadOnlyList<WizardPageViewModelBase> Pages { get; private set; } = null!;

    public WizardPageViewModelBase? CurrentPage =>
        Pages.Count > 0 && CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count ? Pages[CurrentPageIndex] : null;

    public bool CanGoBack => CurrentPage?.CanGoBack is true && CurrentPageIndex > 0;

    public bool IsLastPage => Pages.Count > 0 && CurrentPageIndex == Pages.Count - 1;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public async Task PrepareForShowAsync(WizardOptionKinds? singleOption = null)
    {
        State.Reset();

        if (singleOption is { } kinds)
        {
            Pages = [ExecutionPage];
            foreach (var option in GameConfig.WizardOptions)
            {
                option.IsSelected = option.Kinds == kinds;
            }
        }
        else
        {
            Pages = [GamePathPage, RolePage, ExecutionPage];
        }

        CurrentPageIndex = 0;
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsLastPage));

        foreach (var page in Pages)
        {
            await page.InitializeAsync().ConfigureAwait(true);
        }

        if (singleOption is not null && CurrentPage is { } current)
        {
            current.OnEnterAsync().SafeFireAndForget(ex => Logger.ZLogError(ex, $"Wizard OnEnter failed"));
        }
    }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(true);

        Logger.ZLogInformation($"{nameof(WizardDialogViewModel)} Initialized");
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (IsLastPage)
        {
            if (State.AllSucceeded)
            {
                GameConfig.SetupCompleted = true;
            }
            else
            {
                Logger.ZLogWarning($"Wizard closed without all steps succeeding");
            }

            Close();
            return;
        }

        CurrentPageIndex++;

        if (CurrentPage is { } page)
        {
            await page.OnEnterAsync().ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (CurrentPageIndex > 0)
        {
            CurrentPageIndex--;
        }
    }

    #region Injections

    [UsedImplicitly]
    public required GameConfig GameConfig { get; init; }

    [UsedImplicitly]
    public required ILogger<WizardDialogViewModel> Logger { get; init; }

    [UsedImplicitly]
    public required ExecutionPageViewModel ExecutionPage { get; init; }

    [UsedImplicitly]
    public required GamePathPageViewModel GamePathPage { get; init; }

    [UsedImplicitly]
    public required RolePageViewModel RolePage { get; init; }

    [UsedImplicitly]
    public required WizardState State { get; init; }

    #endregion Injections
}