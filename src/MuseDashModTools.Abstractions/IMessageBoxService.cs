using Ursa.Controls;

namespace MuseDashModTools.Abstractions;

public interface IMessageBoxService
{
    #region Confirm

    // Normal
    Task<MessageBoxResult> WarningConfirmAsync(string message);
    Task<MessageBoxResult> WarningConfirmAsync(string message, params ReadOnlySpan<object> args);
    Task<MessageBoxResult> NoticeConfirmAsync(string message);
    Task<MessageBoxResult> NoticeConfirmAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task<MessageBoxResult> NoticeConfirmOverlayAsync(string message);
    Task<MessageBoxResult> NoticeConfirmOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Error

    // Normal
    Task<MessageBoxResult> ErrorAsync(string message);
    Task<MessageBoxResult> ErrorAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task<MessageBoxResult> ErrorOverlayAsync(string message);
    Task<MessageBoxResult> ErrorOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion

    #region Notice

    // Normal
    Task<MessageBoxResult> NoticeAsync(string message);
    Task<MessageBoxResult> NoticeAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task<MessageBoxResult> NoticeOverlayAsync(string message);

    #endregion

    #region Success

    // Normal
    Task<MessageBoxResult> SuccessAsync(string message);
    Task<MessageBoxResult> SuccessAsync(string message, params ReadOnlySpan<object> args);

    // Overlay
    Task<MessageBoxResult> SuccessOverlayAsync(string message);
    Task<MessageBoxResult> SuccessOverlayAsync(string message, params ReadOnlySpan<object> args);

    #endregion
}