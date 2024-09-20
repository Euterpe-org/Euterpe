namespace MuseDashModTools.Utils;

public static class MessageBoxUtils
{
    #region Confirm MessageBox

    public static Task<MessageBoxResult> WarningConfirmMessageBoxAsync(string message, string title = "Warning") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Warning, MessageBoxButton.YesNo);

    public static Task<MessageBoxResult> FormatWarningConfirmMessageBoxAsync(string message, params object[] args) =>
        WarningConfirmMessageBoxAsync(string.Format(message, args));

    public static Task<MessageBoxResult> NoticeConfirmMessageBoxAsync(string message, string title = "Notice") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Information, MessageBoxButton.YesNo);

    public static Task<MessageBoxResult> FormatNoticeConfirmMessageBoxAsync(string message, params object[] args) =>
        NoticeConfirmMessageBoxAsync(string.Format(message, args));

    #endregion

    #region Error MessageBox

    // Normal
    public static Task<MessageBoxResult> ErrorMessageBoxAsync(string message, string title = "Error") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Error);

    public static Task<MessageBoxResult> FormatErrorMessageBoxAsync(Exception ex) =>
        ErrorMessageBoxAsync(ex.ToString());

    public static Task<MessageBoxResult> FormatErrorMessageBoxAsync(string message, params object[] args) =>
        ErrorMessageBoxAsync(string.Format(message, args));

    // Overlay
    public static Task<MessageBoxResult> ErrorMessageBoxOverlayAsync(string message, string title = "Error") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Error, button: MessageBoxButton.OK);

    public static Task<MessageBoxResult> FormatErrorMessageBoxOverlayAsync(Exception ex) =>
        ErrorMessageBoxOverlayAsync(ex.ToString());

    public static Task<MessageBoxResult> FormatErrorMessageBoxOverlayAsync(string message, params object[] args) =>
        ErrorMessageBoxOverlayAsync(string.Format(message, args));

    #endregion

    #region Notice MessageBox

    // Normal
    public static Task<MessageBoxResult> NoticeMessageBoxAsync(string message, string title = "Notice") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Information);

    public static Task<MessageBoxResult> FormatNoticeMessageBoxAsync(string message, params object[] args) =>
        NoticeMessageBoxAsync(string.Format(message, args));

    // Overlay
    public static Task<MessageBoxResult> NoticeMessageBoxOverlayAsync(string message, string title = "Notice") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Information, button: MessageBoxButton.OK);

    #endregion

    #region Success MessageBox

    // Normal
    public static Task<MessageBoxResult> SuccessMessageBoxAsync(string message, string title = "Success") =>
        MessageBox.ShowAsync(message, title, MessageBoxIcon.Success);

    public static Task<MessageBoxResult> FormatSuccessMessageBoxAsync(string message, params object[] args) =>
        SuccessMessageBoxAsync(string.Format(message, args));

    // Overlay
    public static Task<MessageBoxResult> SuccessMessageBoxOverlayAsync(string message, string title = "Success") =>
        MessageBox.ShowOverlayAsync(message, title, icon: MessageBoxIcon.Success, button: MessageBoxButton.OK);

    public static Task<MessageBoxResult> FormatSuccessMessageBoxOverlayAsync(string message, params object[] args) =>
        SuccessMessageBoxOverlayAsync(string.Format(message, args));

    #endregion
}