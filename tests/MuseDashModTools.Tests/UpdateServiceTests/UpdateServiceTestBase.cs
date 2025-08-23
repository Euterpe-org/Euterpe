using RichardSzalay.MockHttp;

namespace MuseDashModTools.Tests.UpdateServiceTests;

public abstract class UpdateServiceTestBase : IDisposable
{
    protected const string AppVersion = BuildInfo.AppVersion;
    protected const string LowerStableVersion = "0.0.1";
    protected const string LowerPrereleaseVersion = "0.0.1-rc1";
    protected const string HigherStableVersion = "999.0.0";
    protected const string HigherPrereleaseVersion = "999.0.1-rc1";

    internal readonly TestLogger<UpdateService> Logger = new();
    internal readonly MockHttpMessageHandler MockHttp = new();
    protected virtual Config Config { get; } = new();

    public void Dispose()
    {
        Logger.Dispose();
        MockHttp.Dispose();
    }
}