namespace Euterpe.Contracts.Account;

[PublicAPI]
public record TokenPayload(string AccessToken, string RefreshToken);