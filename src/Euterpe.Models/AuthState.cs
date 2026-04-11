using Euterpe.Contracts.Account;

namespace Euterpe.Models;

public sealed partial class AuthState : ObservableObject
{
    [ObservableProperty]
    public partial UserInfo? CurrentUser { get; set; }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset AccessTokenExpiry { get; set; }

    public bool HasSessionTokens => AccessToken is not null && RefreshToken is not null;

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        AccessTokenExpiry = default;
        CurrentUser = null;
        IsLoggedIn = false;
    }
}