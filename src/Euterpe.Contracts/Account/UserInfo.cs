namespace Euterpe.Contracts.Account;

[PublicAPI]
public sealed record UserInfo(
    long Uid,
    int Role,
    string Email,
    string Nickname,
    string? AvatarUrl,
    bool Banned,
    bool HasGithub,
    bool HasGoogle);