namespace Euterpe.Abstractions;

public interface INotificationService
{
    #region Success

    // Normal
    void Success(string content, TimeSpan? expiration = null);
    void Success(string content, params ReadOnlySpan<object> args);

    // Light
    void SuccessLight(string content, TimeSpan? expiration = null);
    void SuccessLight(string content, params ReadOnlySpan<object> args);

    #endregion Success

    #region Notice

    // Normal
    void Notice(string content, TimeSpan? expiration = null);
    void Notice(string content, params ReadOnlySpan<object> args);

    // Light
    void NoticeLight(string content, TimeSpan? expiration = null);
    void NoticeLight(string content, params ReadOnlySpan<object> args);

    #endregion Notice

    #region Error

    // Normal
    void Error(string content, TimeSpan? expiration = null);
    void Error(string content, params ReadOnlySpan<object> args);

    // Light
    void ErrorLight(string content, TimeSpan? expiration = null);
    void ErrorLight(string content, params ReadOnlySpan<object> args);

    #endregion Error

    #region Warning

    // Normal
    void Warning(string content, TimeSpan? expiration = null);
    void Warning(string content, params ReadOnlySpan<object> args);

    // Light
    void WarningLight(string content, TimeSpan? expiration = null);
    void WarningLight(string content, params ReadOnlySpan<object> args);

    #endregion Warning
}
