namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record LoopbackCallbackResult(string? Code, string? State, string? Error);