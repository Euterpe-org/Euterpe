using Euterpe.Shared.Primitives;

namespace Euterpe.Abstractions;

public interface IAuthService
{
    /// <summary>
    ///     Gate that opens when the user has successfully logged in.
    /// </summary>
    AsyncGate Ready { get; }

    /// <summary>
    ///     Start the login flow by opening the system browser.
    /// </summary>
    Task LoginAsync();

    /// <summary>
    ///     Handle the deep link callback with the authorization code.
    /// </summary>
    Task HandleAuthCallbackAsync(string code);

    /// <summary>
    ///     Log out the current user.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    ///     Get a valid access token, refreshing if necessary.
    /// </summary>
    /// <returns>Access token, or null if not logged in.</returns>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    ///     Try to restore the session from stored tokens on startup.
    /// </summary>
    /// <returns>True if session was restored successfully.</returns>
    Task<bool> TryRestoreSessionAsync();
}
