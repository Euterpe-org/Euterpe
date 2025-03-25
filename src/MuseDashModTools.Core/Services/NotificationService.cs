using Avalonia.Controls.Notifications;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace MuseDashModTools.Core;

internal sealed class NotificationService : INotificationService
{
    #region Injections

    [UsedImplicitly]
    public required WindowNotificationManager WindowNotificationManager { get; init; }

    #endregion Injections

    #region Success

    // Normal
    public void Success(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Success, content), NotificationType.Success);

    public void Success(string content, params ReadOnlySpan<object> args) =>
        Success(string.Format(content, args));

    // Light
    public void SuccessLight(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Success, content), NotificationType.Success, classes: ["Light"]);

    public void SuccessLight(string content, params ReadOnlySpan<object> args) =>
        SuccessLight(string.Format(content, args));

    #endregion Success

    #region Notice

    // Normal
    public void Notice(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Notice, content), NotificationType.Information);

    public void Notice(string content, params ReadOnlySpan<object> args) =>
        Notice(string.Format(content, args));

    // Light
    public void NoticeLight(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Notice, content), NotificationType.Information, classes: ["Light"]);

    public void NoticeLight(string content, params ReadOnlySpan<object> args) =>
        NoticeLight(string.Format(content, args));

    #endregion Notice

    #region Error

    // Normal
    public void Error(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Error, content), NotificationType.Error);

    public void Error(string content, params ReadOnlySpan<object> args) =>
        Error(string.Format(content, args));

    // Light
    public void ErrorLight(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Error, content), NotificationType.Error, classes: ["Light"]);

    public void ErrorLight(string content, params ReadOnlySpan<object> args) =>
        ErrorLight(string.Format(content, args));

    #endregion Error

    #region Warning

    // Normal
    public void Warning(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Warning, content), NotificationType.Warning);

    public void Warning(string content, params ReadOnlySpan<object> args) =>
        Warning(string.Format(content, args));

    // Light
    public void WarningLight(string content) =>
        WindowNotificationManager.Show(new Notification(Title_Warning, content), NotificationType.Warning, classes: ["Light"]);

    public void WarningLight(string content, params ReadOnlySpan<object> args) =>
        WarningLight(string.Format(content, args));

    #endregion Warning
}