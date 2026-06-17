namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record TokenPayload(string AccessToken, string RefreshToken);
