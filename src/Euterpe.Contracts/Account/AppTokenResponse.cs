namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record AppTokenResponse(string AccessToken, string RefreshToken, UserInfo Me);