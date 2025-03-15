using Ursa.Controls;

namespace MuseDashModTools.Abstractions;

public interface IMessageBoxService
{
    #region Confirm MessageBox

    public Task<MessageBoxResult> WarningConfirmAsync(string message);

    public Task<MessageBoxResult> WarningConfirmAsync(string message, params ReadOnlySpan<object> args);

    public Task<MessageBoxResult> NoticeConfirmAsync(string message);

    public Task<MessageBoxResult> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Error MessageBox

    // Normal
    public Task<MessageBoxResult> ErrorAsync(string message);

    public Task<MessageBoxResult> ErrorAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    public Task<MessageBoxResult> ErrorOverlayAsync(string message);

    public Task<MessageBoxResult> ErrorOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Notice MessageBox

    // Normal
    public Task<MessageBoxResult> NoticeAsync(string message);

    public Task<MessageBoxResult> NoticeAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    public Task<MessageBoxResult> NoticeOverlayAsync(string message);

    #endregion

    #region Success MessageBox

    // Normal
    public Task<MessageBoxResult> SuccessAsync(string message);

    public Task<MessageBoxResult> SuccessAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    public Task<MessageBoxResult> SuccessOverlayAsync(string message);
    public Task<MessageBoxResult> SuccessOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion
}