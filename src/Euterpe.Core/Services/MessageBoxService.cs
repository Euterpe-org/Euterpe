using Avalonia.Threading;
using Ursa.Controls;

namespace Euterpe.Core;

internal sealed class MessageBoxService : IMessageBoxService
{
    private static Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxIcon icon, MessageBoxButton button) =>
        Dispatcher.UIThread.CheckAccess()
            ? MessageBox.ShowAsync(message, title, icon, button)
            : Dispatcher.UIThread.InvokeAsync(() => MessageBox.ShowAsync(message, title, icon, button));

    private static Task<MessageBoxResult> ShowOverlayAsync(string message, string title, MessageBoxIcon icon, MessageBoxButton button) =>
        Dispatcher.UIThread.CheckAccess()
            ? OverlayMessageBox.ShowAsync(message, title, icon: icon, button: button)
            : Dispatcher.UIThread.InvokeAsync(() => OverlayMessageBox.ShowAsync(message, title, icon: icon, button: button));

    #region Confirm

    // Normal
    public Task<MessageBoxResult> WarningConfirmAsync(string message) =>
        ShowAsync(message, Title_Warning, MessageBoxIcon.Warning, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> WarningConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        WarningConfirmAsync(string.Format(message, args));

    public Task<MessageBoxResult> NoticeConfirmAsync(string message) =>
        ShowAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeConfirmAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> NoticeConfirmOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> NoticeConfirmOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeConfirmOverlayAsync(string.Format(message, args));

    #endregion

    #region Error

    // Normal
    public Task<MessageBoxResult> ErrorAsync(string message) =>
        ShowAsync(message, Title_Error, MessageBoxIcon.Error, MessageBoxButton.OK);

    public Task<MessageBoxResult> ErrorAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> ErrorOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Error, MessageBoxIcon.Error, MessageBoxButton.OK);

    public Task<MessageBoxResult> ErrorOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorOverlayAsync(string.Format(message, args));

    #endregion

    #region Notice

    // Normal
    public Task<MessageBoxResult> NoticeAsync(string message) =>
        ShowAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.OK);

    public Task<MessageBoxResult> NoticeAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> NoticeOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Notice, MessageBoxIcon.Information, MessageBoxButton.OK);

    #endregion

    #region Success

    // Normal
    public Task<MessageBoxResult> SuccessAsync(string message) =>
        ShowAsync(message, Title_Success, MessageBoxIcon.Success, MessageBoxButton.OK);

    public Task<MessageBoxResult> SuccessAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> SuccessOverlayAsync(string message) =>
        ShowOverlayAsync(message, Title_Success, MessageBoxIcon.Success, MessageBoxButton.OK);

    public Task<MessageBoxResult> SuccessOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessOverlayAsync(string.Format(message, args));

    #endregion
}
