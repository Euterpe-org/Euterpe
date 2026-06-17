using Euterpe.Contracts.Account;

namespace Euterpe.Abstractions;

public interface IGameUidProvider
{
    /// <summary>
    ///     Get MuseDash UID
    /// </summary>
    /// <returns></returns>
    Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync();
}
