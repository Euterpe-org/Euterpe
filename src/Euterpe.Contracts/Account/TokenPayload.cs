namespace Euterpe.Contracts.Account;

[PublicAPI]
public readonly record struct TokenPayload(string AccessToken, string RefreshToken);