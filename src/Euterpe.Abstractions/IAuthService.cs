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
    ///     Concurrent callers passing the same <paramref name="staleToken" /> are deduplicated:
    ///     only the first one performs the refresh, subsequent callers receive the already-renewed token.
    /// </summary>
    /// <param name="staleToken">The access token that the caller observed as invalid.</param>
    Task<string> RenewAccessTokenAsync(string staleToken);

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