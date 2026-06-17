using Euterpe.Contracts.Account;

namespace Euterpe.Core;

[SupportedOSPlatform(nameof(OSPlatform.OSX))]
internal sealed class MacOsGameUidProvider : IGameUidProvider
{
    public Task<MuseDashUidRequest?> GetMuseDashUidRequestAsync() => throw new NotSupportedException();
}
