using Euterpe.Models.Progress;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Share;

[Register]
public sealed partial class ShareImportDialogViewModel : ViewModelBase, IDialogContext
{
    private GameSharePackage? _pendingPackage;

    [ObservableProperty]
    public partial string ShareText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ValidationMessage { get; set; }

    [ObservableProperty]
    public partial bool CanImport { get; set; }

    [ObservableProperty]
    public partial bool IsImporting { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ResultSummary { get; set; }

    public void Prepare(string? shareText = null)
    {
        CancelImport();
        _pendingPackage = null;
        ResultSummary = null;
        IsImporting = false;
        Progress = 0;
        ProgressLabel = string.Empty;
        var preparedText = shareText ?? string.Empty;
        if (ShareText == preparedText)
        {
            ValidateShareText(preparedText);
        }
        else
        {
            ShareText = preparedText;
        }
    }

    [RelayCommand]
    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        if (!CanImport || _pendingPackage is not { } package)
        {
            return;
        }

        IsImporting = true;
        ResultSummary = null;
        Progress = 0;
        ProgressLabel = string.Empty;
        try
        {
            var progress = new Progress<BatchProgress>(report =>
            {
                Progress = report.Percentage;
                ProgressLabel = report.CountDisplay;
            });
            var result = await GameShareService.ImportAsync(package, progress, cancellationToken).ConfigureAwait(true);
            ResultSummary = FormatResult(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ResultSummary = XAML.Share_Import_Canceled;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to import game share package");
            ResultSummary = XAML.Share_Import_Failed;
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    public void CancelImport() => ImportCommand.Cancel();

    partial void OnShareTextChanged(string value) => ValidateShareText(value);

    private void ValidateShareText(string value)
    {
        ResultSummary = null;
        _pendingPackage = GameShareService.TryParseShareLink(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            CanImport = false;
            ValidationMessage = null;
        }
        else if (_pendingPackage is not { } package)
        {
            CanImport = false;
            ValidationMessage = XAML.Share_Import_Invalid;
        }
        else if (package.GameId != Config.ActiveGame)
        {
            CanImport = false;
            var gameName = Config.Games.FirstOrDefault(game => game.Id == package.GameId)?.DisplayName ?? package.GameId.ToString();
            ValidationMessage = string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_WrongGame, gameName);
        }
        else
        {
            CanImport = true;
            ValidationMessage = string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Preview,
                package.ChartIds.Length, package.Mods.Length);
        }
    }

    private static string FormatResult(GameShareImportResult result)
    {
        var charts = result.ChartResults;
        var mods = result.ModResults;
        return string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Result,
            Count(charts, BulkItemOutcome.Added), Count(charts, BulkItemOutcome.AlreadyPresent), Count(charts, BulkItemOutcome.Failed),
            Count(mods, BulkItemOutcome.Added), Count(mods, BulkItemOutcome.AlreadyPresent),
            Count(mods, BulkItemOutcome.Skipped) + Count(mods, BulkItemOutcome.Failed));

        static int Count(IReadOnlyList<BulkItemResult> items, BulkItemOutcome outcome) =>
            items.Count(item => item.Outcome == outcome);
    }

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    #region Injections

    public required IGameShareService GameShareService { get; init; }
    public required Config Config { get; init; }
    public required ILogger<ShareImportDialogViewModel> Logger { get; init; }

    #endregion Injections
}
