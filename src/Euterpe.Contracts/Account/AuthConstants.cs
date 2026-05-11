namespace Euterpe.Contracts.Account;

[PublicAPI]
public static class AuthConstants
{
    // Server-side access token lifetime is 15 minutes; 14 leaves a 1-minute buffer for clock skew and network delay.
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(14);
}