using Avalonia.Input.Platform;
using Euterpe.Core.Proxies;
using Euterpe.Models.Progress;
using Irihi.Avalonia.Shared.Contracts;

namespace Euterpe.Features.Share;

[Register]
public sealed partial class ShareImportDialogViewModel : ViewModelBase, IDialogContext
{
    private GameSharePackage? _pendingPackage;

    [ObservableProperty]
    public partial string? StatusMessage { get; private set; }

    [ObservableProperty]
    public partial bool IsStatusWarning { get; private set; }

    [ObservableProperty]
    public partial bool CanImport { get; private set; }

    [ObservableProperty]
    public partial bool IsImporting { get; private set; }

    [ObservableProperty]
    public partial double Progress { get; private set; }

    [ObservableProperty]
    public partial string ProgressLabel { get; private set; } = string.Empty;

    public event EventHandler<object?>? RequestClose;

    public void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    public async Task PrepareAsync(string? shareText = null)
    {
        CancelImport();
        Inspect(shareText ?? await ReadClipboardAsync().ConfigureAwait(true));
    }

    [RelayCommand]
    private async Task RefreshAsync() => Inspect(await ReadClipboardAsync().ConfigureAwait(true));

    [RelayCommand]
    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        if (_pendingPackage is not { } package)
        {
            return;
        }

        IsImporting = true;
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
            SetStatus(null, FormatResult(result), result.Any(static item => item.Outcome is BulkItemOutcome.Failed));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            SetStatus(null, XAML.Share_Import_Canceled, warning: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to import game share package");
            SetStatus(null, XAML.Share_Import_Failed, warning: true);
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    public void CancelImport() => ImportCommand.Cancel();

    private async Task<string> ReadClipboardAsync()
    {
        try
        {
            return await TopLevel.Clipboard!.TryGetTextAsync().ConfigureAwait(true) ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read the clipboard");
            return string.Empty;
        }
    }

    private void SetStatus(GameSharePackage? package, string message, bool warning)
    {
        _pendingPackage = package;
        CanImport = package is not null;
        IsStatusWarning = warning;
        StatusMessage = message;
    }

    private void Inspect(string shareText)
    {
        var package = GameShareService.TryParseShareLink(shareText);

        if (string.IsNullOrWhiteSpace(shareText))
        {
            SetStatus(null, XAML.Share_Import_Empty, warning: true);
        }
        else if (package is null)
        {
            SetStatus(null, XAML.Share_Import_Invalid, warning: true);
        }
        else if (package.GameId != Config.ActiveGame)
        {
            var gameName = Config.Games.FirstOrDefault(game => game.Id == package.GameId)?.DisplayName ?? package.GameId.ToString();
            SetStatus(null, string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_WrongGame, gameName), warning: true);
        }
        else
        {
            SetStatus(package, string.Format(CultureInfo.CurrentCulture, XAML.Share_Import_Preview, package.ChartIds.Length), warning: false);
        }
    }

    private static string FormatResult(IReadOnlyList<BulkItemResult> result)
    {
        string[] phrases =
        [
            Phrase(BulkItemOutcome.Added, XAML.Share_Import_Result_Added),
            Phrase(BulkItemOutcome.AlreadyPresent, XAML.Share_Import_Result_Present),
            Phrase(BulkItemOutcome.Failed, XAML.Share_Import_Result_Failed)
        ];

        return string.Join(" · ", phrases.Where(static phrase => phrase.Length > 0));

        string Phrase(BulkItemOutcome outcome, string format)
        {
            var count = result.Count(item => item.Outcome == outcome);
            return count is 0 ? string.Empty : string.Format(CultureInfo.CurrentCulture, format, count);
        }
    }

    #region Injections

    public required IGameShareService GameShareService { get; init; }
    public required Config Config { get; init; }
    public required ILogger<ShareImportDialogViewModel> Logger { get; init; }
    public required TopLevelProxy TopLevel { get; init; }

    #endregion Injections
}
