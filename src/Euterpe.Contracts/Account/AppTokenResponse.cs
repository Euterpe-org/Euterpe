namespace Euterpe.Contracts.Account;

[PublicAPI]
public readonly record struct AppTokenResponse(string AccessToken, string RefreshToken, UserInfo Me);