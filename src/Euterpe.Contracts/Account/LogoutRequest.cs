namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record LogoutRequest(string RefreshToken);
