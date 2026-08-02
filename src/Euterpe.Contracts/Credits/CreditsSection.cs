namespace Euterpe.Contracts.Credits;

[PublicAPI]
public sealed record CreditsSection(string Title, CreditsPerson[] Items);
