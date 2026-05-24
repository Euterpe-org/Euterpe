using System.Runtime.InteropServices;
using Avalonia.Input.Platform;
using Euterpe.Core.Proxies;

namespace Euterpe.Shell;

public sealed partial class CrashWindowViewModel : ViewModelBase, IDialog<bool>
{
    private const string FeedbackUri = "https://github.com/Euterpe-org/Euterpe/issues/new/choose";

    public string EnvironmentInfo { get; } =
        $"Euterpe {DisplayVersion} · {RuntimeInformation.OSDescription.Trim()} ({RuntimeInformation.OSArchitecture}) · {RuntimeInformation.FrameworkDescription}";

    [ObservableProperty]
    public partial string CrashTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ExceptionType { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ExceptionMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ExceptionDetails { get; set; } = string.Empty;

    public event EventHandler<bool>? RequestClose;
    public void Close(bool result) => RequestClose?.Invoke(this, result);

    public void SetException(Exception ex)
    {
        ExceptionType = ex.GetType().FullName ?? nameof(Exception);
        ExceptionMessage = ex.Message;
        ExceptionDetails = ex.ToString();
        CrashTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        try
        {
            if (TopLevel.Clipboard is null)
            {
                throw new InvalidOperationException("Clipboard unavailable.");
            }

            var copyText = $"{EnvironmentInfo}{Environment.NewLine}{CrashTime}{Environment.NewLine}{Environment.NewLine}{ExceptionDetails}";
            await TopLevel.Clipboard.SetTextAsync(copyText).ConfigureAwait(true);
            await MessageBoxService.SuccessAsync(MessageBox_Content_CrashDialog_CopySuccess).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Crash dialog clipboard copy failed");
            await MessageBoxService.ErrorAsync(MessageBox_Content_CrashDialog_CopyFailed).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private void OpenLog() => Launcher.RevealFile(LogFilePath);

    [RelayCommand]
    private async Task FeedbackAsync() => await Launcher.OpenUriAsync(FeedbackUri).ConfigureAwait(false);

    [RelayCommand]
    private void Continue() => Close(true);

    [RelayCommand]
    private void Exit() => Close(false);

    #region Injections

    public required ILogger<CrashWindowViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required TopLevelProxy TopLevel { get; init; }

    #endregion Injections
}