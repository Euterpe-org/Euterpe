namespace MuseDashModTools.Core;

internal sealed class NotificationService : INotificationService
{
    #region Notice

    public void Notice(string title, string content) => WindowNotificationManager.Show(new Notification(title, content));

    #endregion Notice

    #region Injections

    [UsedImplicitly]
    public required WindowNotificationManager WindowNotificationManager { get; init; }

    [UsedImplicitly]
    public required ILogger<NotificationService> Logger { get; init; }

    #endregion Injections
}