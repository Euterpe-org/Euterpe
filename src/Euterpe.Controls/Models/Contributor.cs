namespace Euterpe.Controls.Models;

public sealed record Contributor(string Name, string AvatarUrl, string? Description, ContributorLink[]? Links);
