namespace Euterpe.Contracts.Account;

[PublicAPI]
public readonly record struct UserInfo(
    long Uid,
    int Role,
    string Email,
    string Nickname,
    string? AvatarUrl,
    bool Banned,
    bool HasGithub,
    bool HasGoogle);