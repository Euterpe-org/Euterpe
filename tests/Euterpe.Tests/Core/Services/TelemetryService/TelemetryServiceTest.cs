using System.Net;
using Euterpe.Contracts.Telemetry;
using Euterpe.Core.Http.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests;

[Category("TelemetryServiceTests")]
[TestSubject(typeof(TelemetryService))]
public sealed class TelemetryServiceTest
{
    private static TelemetryService NewService(
        ITelemetryApiClient? client = null,
        IPlatformInfo? platformInfo = null) => new()
    {
        TelemetryApiClient = client ?? ITelemetryApiClient.Mock(),
        PlatformInfo = platformInfo ?? StubPlatformInfo(),
        Logger = NullLogger<TelemetryService>.Instance
    };

    private static IPlatformInfo StubPlatformInfo()
    {
        var mock = IPlatformInfo.Mock();
        mock.OsString.Returns("linux");
        return mock;
    }

    [Test]
    public async Task TrackSessionAsync_SendsPayloadWithPlatformAndVersion()
    {
        var captured = new List<SessionEvent>();
        var client = ITelemetryApiClient.Mock();
        client.TrackSessionAsync(Any<SessionEvent>(), Any<CancellationToken>())
            .Callback((p, _) => captured.Add(p));
        var platform = IPlatformInfo.Mock();
        platform.OsString.Returns("linux");
        platform.ArchitectureString.Returns("x64");

        await NewService(client, platform).TrackSessionAsync();

        var sent = captured.Single();
        using var _ = Assert.Multiple();
        await Assert.That(sent.Platform).IsEqualTo("linux");
        await Assert.That(sent.Arch).IsEqualTo("x64");
        await Assert.That(sent.AppVersion).IsEqualTo(AppVersion);
        await Assert.That(sent.Country).IsNotNull();
    }

    [Test]
    public async Task TrackSessionAsync_ApiClientThrows_SwallowsException()
    {
        var client = ITelemetryApiClient.Mock();
        client.TrackSessionAsync(Any<SessionEvent>(), Any<CancellationToken>())
            .Throws(new HttpRequestException("server unreachable"));

        var act = async () => await NewService(client).TrackSessionAsync();
        await Assert.That(act).ThrowsNothing();
    }

    [Test]
    public async Task TrackSessionAsync_CallsApiClientOnce()
    {
        var client = ITelemetryApiClient.Mock();
        client.TrackSessionAsync(Any<SessionEvent>(), Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NoContent));

        await NewService(client).TrackSessionAsync();

        client.TrackSessionAsync(Any<SessionEvent>(), Any<CancellationToken>()).WasCalled(Times.Once);
    }
}