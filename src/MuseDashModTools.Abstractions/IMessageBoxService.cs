using Ursa.Controls;

namespace MuseDashModTools.Abstractions;

public interface IMessageBoxService
{
    #region Confirm MessageBox

    public Task<MessageBoxResult> WarningConfirmMessageBoxAsync(string message, string title = "Warning");

    public Task<MessageBoxResult> FormatWarningConfirmMessageBoxAsync(string message, params ReadOnlySpan<object?> args);

    public Task<MessageBoxResult> NoticeConfirmMessageBoxAsync(string message, string title = "Notice");

    public Task<MessageBoxResult> FormatNoticeConfirmMessageBoxAsync(string message, params ReadOnlySpan<object?> args);

    #endregion

    #region Error MessageBox

    // Normal
    public Task<MessageBoxResult> ErrorMessageBoxAsync(string message, string title = "Error");

    public Task<MessageBoxResult> FormatErrorMessageBoxAsync(string message, params ReadOnlySpan<object?> args);

    // Overlay
    public Task<MessageBoxResult> ErrorMessageBoxOverlayAsync(string message, string title = "Error");

    public Task<MessageBoxResult> FormatErrorMessageBoxOverlayAsync(string message, params ReadOnlySpan<object?> args);

    #endregion

    #region Notice MessageBox

    // Normal
    public Task<MessageBoxResult> NoticeMessageBoxAsync(string message, string title = "Notice");

    public Task<MessageBoxResult> FormatNoticeMessageBoxAsync(string message, params ReadOnlySpan<object?> args);

    // Overlay
    public Task<MessageBoxResult> NoticeMessageBoxOverlayAsync(string message, string title = "Notice");

    #endregion

    #region Success MessageBox

    // Normal
    public Task<MessageBoxResult> SuccessMessageBoxAsync(string message, string title = "Success");

    public Task<MessageBoxResult> FormatSuccessMessageBoxAsync(string message, params ReadOnlySpan<object?> args);

    // Overlay
    public Task<MessageBoxResult> SuccessMessageBoxOverlayAsync(string message, string title = "Success");
    public Task<MessageBoxResult> FormatSuccessMessageBoxOverlayAsync(string message, params ReadOnlySpan<object?> args);

    #endregion
}