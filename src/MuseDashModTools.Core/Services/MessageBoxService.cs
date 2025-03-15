namespace MuseDashModTools.Core;

internal sealed class MessageBoxService : IMessageBoxService
{
    #region Confirm MessageBox

    public Task<MessageBoxResult> WarningConfirmAsync(string message) =>
        MessageBox.ShowAsync(message, MsgBox_Title_Warning, MessageBoxIcon.Warning, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> WarningConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        WarningConfirmAsync(string.Format(message, args));

    public Task<MessageBoxResult> NoticeConfirmAsync(string message) =>
        MessageBox.ShowAsync(message, MsgBox_Title_Notice, MessageBoxIcon.Information, MessageBoxButton.YesNo);

    public Task<MessageBoxResult> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeConfirmAsync(string.Format(message, args));

    #endregion

    #region Error MessageBox

    // Normal
    public Task<MessageBoxResult> ErrorAsync(string message) =>
        MessageBox.ShowAsync(message, MsgBox_Title_Error, MessageBoxIcon.Error);

    public Task<MessageBoxResult> ErrorAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> ErrorOverlayAsync(string message) =>
        MessageBox.ShowOverlayAsync(message, MsgBox_Title_Error, icon: MessageBoxIcon.Error, button: MessageBoxButton.OK);

    public Task<MessageBoxResult> ErrorOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        ErrorOverlayAsync(string.Format(message, args));

    #endregion

    #region Notice MessageBox

    // Normal
    public Task<MessageBoxResult> NoticeAsync(string message) =>
        MessageBox.ShowAsync(message, MsgBox_Title_Notice, MessageBoxIcon.Information);

    public Task<MessageBoxResult> NoticeAsync(string message, params ReadOnlySpan<object> args) =>
        NoticeAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> NoticeOverlayAsync(string message) =>
        MessageBox.ShowOverlayAsync(message, MsgBox_Title_Notice, icon: MessageBoxIcon.Information, button: MessageBoxButton.OK);

    #endregion

    #region Success MessageBox

    // Normal
    public Task<MessageBoxResult> SuccessAsync(string message) =>
        MessageBox.ShowAsync(message, MsgBox_Title_Success, MessageBoxIcon.Success);

    public Task<MessageBoxResult> SuccessAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessAsync(string.Format(message, args));

    // Overlay
    public Task<MessageBoxResult> SuccessOverlayAsync(string message) =>
        MessageBox.ShowOverlayAsync(message, MsgBox_Title_Success, icon: MessageBoxIcon.Success, button: MessageBoxButton.OK);

    public Task<MessageBoxResult> SuccessOverlayAsync(string message, params ReadOnlySpan<object> args) =>
        SuccessOverlayAsync(string.Format(message, args));

    #endregion
}