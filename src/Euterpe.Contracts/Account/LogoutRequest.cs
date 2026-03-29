namespace Euterpe.Contracts.Account;

[PublicAPI]
public readonly record struct LogoutRequest(string RefreshToken);