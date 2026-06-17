namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record AppTokenRequest(string ClientId, string Code, string CodeVerifier, string RedirectUri);
