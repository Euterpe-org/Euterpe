using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace Euterpe.Core;

internal sealed class NotificationService : INotificationService
{
    #region Injections

    [UsedImplicitly]
    public required WindowNotificationManager WindowNotificationManager { get; init; }

    #endregion Injections

    private void Show(Notification notification, NotificationType type, string[]? classes = null)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            WindowNotificationManager.Show(notification, type, classes: classes);
        }
        else
        {
            Dispatcher.UIThread.Post(() => WindowNotificationManager.Show(notification, type, classes: classes));
        }
    }

    #region Success

    // Normal
    public void Success(string content) =>
        Show(new Notification(Title_Success, content), NotificationType.Success);

    public void Success(string content, params ReadOnlySpan<object> args) =>
        Success(string.Format(content, args));

    // Light
    public void SuccessLight(string content) =>
        Show(new Notification(Title_Success, content), NotificationType.Success, ["Light"]);

    public void SuccessLight(string content, params ReadOnlySpan<object> args) =>
        SuccessLight(string.Format(content, args));

    #endregion Success

    #region Notice

    // Normal
    public void Notice(string content) =>
        Show(new Notification(Title_Notice, content), NotificationType.Information);

    public void Notice(string content, params ReadOnlySpan<object> args) =>
        Notice(string.Format(content, args));

    // Light
    public void NoticeLight(string content) =>
        Show(new Notification(Title_Notice, content), NotificationType.Information, ["Light"]);

    public void NoticeLight(string content, params ReadOnlySpan<object> args) =>
        NoticeLight(string.Format(content, args));

    #endregion Notice

    #region Error

    // Normal
    public void Error(string content) =>
        Show(new Notification(Title_Error, content), NotificationType.Error);

    public void Error(string content, params ReadOnlySpan<object> args) =>
        Error(string.Format(content, args));

    // Light
    public void ErrorLight(string content) =>
        Show(new Notification(Title_Error, content), NotificationType.Error, ["Light"]);

    public void ErrorLight(string content, params ReadOnlySpan<object> args) =>
        ErrorLight(string.Format(content, args));

    #endregion Error

    #region Warning

    // Normal
    public void Warning(string content) =>
        Show(new Notification(Title_Warning, content), NotificationType.Warning);

    public void Warning(string content, params ReadOnlySpan<object> args) =>
        Warning(string.Format(content, args));

    // Light
    public void WarningLight(string content) =>
        Show(new Notification(Title_Warning, content), NotificationType.Warning, ["Light"]);

    public void WarningLight(string content, params ReadOnlySpan<object> args) =>
        WarningLight(string.Format(content, args));

    #endregion Warning
}