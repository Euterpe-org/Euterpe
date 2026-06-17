namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record UserInfo(
    long Uid,
    int Role,
    string Email,
    string Nickname,
    string? AvatarUrl,
    bool Banned,
    [property: JsonPropertyName("has_github")] bool HasGitHub,
    [property: JsonPropertyName("has_google")] bool HasGoogle);
