using DotNext.Threading;

namespace Euterpe.Abstractions;

public interface IAuthService
{
    /// <summary>
    ///     Gate that opens when the user has successfully logged in.
    /// </summary>
    AsyncManualResetEvent Ready { get; }

    /// <summary>
    ///     Start the login flow by opening the system browser.
    /// </summary>
    Task LoginAsync();

    /// <summary>
    ///     Log out the current user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    ///     Get a usable access token, refreshing it if necessary.
    ///     Waits for login to complete if no session tokens are present.
    /// </summary>
    Task<string> GetAccessTokenAsync();

    /// <summary>
    ///     Force the server to issue a new access token using the current refresh token.
    ///     If the refresh token is rejected, triggers re-login and waits for completion.
    /// </summary>
    Task<string> RenewAccessTokenAsync();

    /// <summary>
    ///     Handle the deep link callback with the authorization code.
    /// </summary>
    Task CompleteLoginAsync(string code);

    /// <summary>
    ///     Try to restore the session from stored tokens on startup.
    /// </summary>
    /// <returns>True if session was restored successfully.</returns>
    Task<bool> RestoreSessionAsync();
}