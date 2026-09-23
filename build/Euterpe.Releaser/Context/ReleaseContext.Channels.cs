namespace Euterpe.Releaser;

public sealed partial class ReleaseContext
{
    public string StableChannel => $"{Rid}-stable";
    public string BetaChannel => $"{Rid}-beta";
    public string PrimaryChannel => Version.IsPrerelease ? BetaChannel : StableChannel;
    public IReadOnlyList<string> BaseChannels => Version.IsPrerelease ? [PrimaryChannel] : [StableChannel, BetaChannel];

    public Dictionary<string, VelopackReleaseBase?> ReleaseBases { get; } = [];

    public IReadOnlyList<string> PackageChannels =>
        Version.IsPrerelease || ReleaseBases.GetValueOrDefault(BetaChannel) is null
            ? [PrimaryChannel]
            : [StableChannel, BetaChannel];

    public bool IsVersionPublished => PackageChannels.All(channel => ReleaseBases.GetValueOrDefault(channel)?.Version == Version.ToString());
}
