namespace MuseDashModTools.Abstractions;

public interface INotificationService
{
    #region Success

    // Normal
    void Success(string content);
    void Success(string content, params ReadOnlySpan<object> args);

    // Light
    void SuccessLight(string content);
    void SuccessLight(string content, params ReadOnlySpan<object> args);

    #endregion Success

    #region Notice

    // Normal
    void Notice(string content);
    void Notice(string content, params ReadOnlySpan<object> args);

    // Light
    void NoticeLight(string content);
    void NoticeLight(string content, params ReadOnlySpan<object> args);

    #endregion Notice

    #region Error

    // Normal
    void Error(string content);
    void Error(string content, params ReadOnlySpan<object> args);

    // Light
    void ErrorLight(string content);
    void ErrorLight(string content, params ReadOnlySpan<object> args);

    #endregion Error

    #region Warning

    // Normal
    void Warning(string content);
    void Warning(string content, params ReadOnlySpan<object> args);

    // Light
    void WarningLight(string content);
    void WarningLight(string content, params ReadOnlySpan<object> args);

    #endregion Warning
}