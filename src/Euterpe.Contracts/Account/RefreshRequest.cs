namespace Euterpe.Contracts.Account;

[PublicAPI]
public readonly record struct RefreshRequest(string RefreshToken);