namespace Euterpe.Contracts.Account;

[PublicAPI]
public record AppTokenResponse(string AccessToken, string RefreshToken, UserInfo Me);