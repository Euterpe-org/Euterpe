namespace MuseDashModTools.Core;

public sealed class MessageBoxService : IMessageBoxService
{
    #region Confirm MessageBox

    public Task<MessageBoxResult> WarningConfirmMessageBoxAsync(string message, string title = "Warning") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Warning, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> FormatWarningConfirmMessageBoxAsync(string message, params ReadOnlySpan<object?> args) =>
        WarningConfirmMessageBoxAsync(string.Format(message, args));

    public Task<MessageBoxResult> NoticeConfirmMessageBoxAsync(string message, string title = "Notice") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Information, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> FormatNoticeConfirmMessageBoxAsync(string message, params ReadOnlySpan<object?> args) =>
        NoticeConfirmMessageBoxAsync(string.Format(message, args));

    #endregion

    #region Error MessageBox

    // Normal
    public Task<MessageBoxResult> ErrorMessageBoxAsync(string message, string title = "Error") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Error);

    public Task<MessageBoxResult> FormatErrorMessageBoxAsync(string message, params ReadOnlySpan<object?> args) =>
        ErrorMessageBoxAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> ErrorMessageBoxOverlayAsync(string message, string title = "Error") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Error, button: MessageBoxButton.OK);

    public Task<MessageBoxResult> FormatErrorMessageBoxOverlayAsync(string message, params ReadOnlySpan<object?> args) =>
        ErrorMessageBoxOverlayAsync(string.Format(message, args));

    #endregion

    #region Notice MessageBox

    // Normal
    public Task<MessageBoxResult> NoticeMessageBoxAsync(string message, string title = "Notice") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Information);

    public Task<MessageBoxResult> FormatNoticeMessageBoxAsync(string message, params ReadOnlySpan<object?> args) =>
        NoticeMessageBoxAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> NoticeMessageBoxOverlayAsync(string message, string title = "Notice") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Information, button: MessageBoxButton.OK);

    #endregion

    #region Success MessageBox

    // Normal
    public Task<MessageBoxResult> SuccessMessageBoxAsync(string message, string title = "Success") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Success);

    public Task<MessageBoxResult> FormatSuccessMessageBoxAsync(string message, params ReadOnlySpan<object?> args) =>
        SuccessMessageBoxAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> SuccessMessageBoxOverlayAsync(string message, string title = "Success") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Success, button: MessageBoxButton.OK);

    public Task<MessageBoxResult> FormatSuccessMessageBoxOverlayAsync(string message, params ReadOnlySpan<object?> args) =>
        SuccessMessageBoxOverlayAsync(string.Format(message, args));

    #endregion
}