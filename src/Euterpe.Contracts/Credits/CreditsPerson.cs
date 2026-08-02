namespace Euterpe.Contracts.Credits;

[PublicAPI]
public sealed record CreditsPerson(string Name, string Avatar, string Description, CreditsPersonLink[] Links);
