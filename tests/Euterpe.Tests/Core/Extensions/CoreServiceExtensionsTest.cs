using Autofac;
using Downloader;
using Euterpe.Core.Extensions;
using Euterpe.Core.Http.Clients;
using Euterpe.Core.Http.Handlers;
using Euterpe.Core.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Euterpe.Tests;

[Category("CoreServiceExtensionsTests")]
[TestSubject(typeof(CoreServiceExtensions))]
public sealed class CoreServiceExtensionsTest
{
    [Test]
    public async Task RegisterLogger_RegistersLoggerServices()
    {
        var services = new ServiceCollection();
        services.RegisterLogger();

        using var _ = Assert.Multiple();
        await Assert.That(services.Any(s => s.ServiceType == typeof(LiveLogProcessor))).IsTrue();
        await Assert.That(services.Any(s => s.ServiceType == typeof(ILoggerFactory))).IsTrue();
    }

    [Test]
    public async Task RegisterHttpClients_RegistersAllHandlersAndDownloadService()
    {
        var services = new ServiceCollection();
        services.RegisterHttpClients();
        var provider = services.BuildServiceProvider();

        using var _ = Assert.Multiple();

        await Assert.That(provider.GetService<XRequestIdHandler>()).IsNotNull();
        await Assert.That(provider.GetService<AuthHeaderHandler>()).IsNotNull();
        await Assert.That(provider.GetService<LoggingHandler>()).IsNotNull();
        await Assert.That(provider.GetService<ServerErrorHandler>()).IsNotNull();
        await Assert.That(provider.GetService<TokenQueryHandler>()).IsNotNull();
        await Assert.That(provider.GetService<IDownloadService>()).IsNotNull();
        await Assert.That(provider.GetService<IHttpClientFactory>()).IsNotNull();
    }

    [Test]
    public async Task RegisterHttpClients_RegistersAllRefitClientServiceDescriptors()
    {
        // Refit clients can't be resolved at runtime in this test setup (the source generator
        // doesn't emit clients into the test assembly), but service-collection registration
        // is what RegisterHttpClients owns — verify the descriptors are present.
        var services = new ServiceCollection();
        services.RegisterHttpClients();

        Type[] expectedRefitClients =
        [
            typeof(IEuterpeAuthClient),
            typeof(IEuterpeAccountClient),
            typeof(IEuterpeDistributionClient),
            typeof(IEuterpeModClient),
            typeof(IEuterpeChartClient),
            typeof(ITelemetryApiClient)
        ];

        using var _ = Assert.Multiple();
        foreach (var t in expectedRefitClients)
        {
            await Assert.That(services.Any(s => s.ServiceType == t)).IsTrue();
        }

        await Assert.That(services.Any(s => s.ServiceType == typeof(EuterpeDownloadClient))).IsTrue();
    }

    [Test]
    [Arguments(GameId.MuseDash, typeof(MuseDashConfig))]
    [Arguments(GameId.MuseDash2, typeof(MuseDash2Config))]
    public async Task RegisterPerGameCoreServices_ResolvesGameConfigForActiveGame(GameId gameId, Type expectedConcrete)
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        builder.RegisterPerGameCoreServices(gameId);

        await using var container = builder.Build();
        var resolved = container.Resolve<GameConfig>();

        await Assert.That(resolved.GetType()).IsEqualTo(expectedConcrete);
    }

    [Test]
    public async Task RegisterAppCoreServices_RegistersAllAppLevelSingletons()
    {
        var builder = new ContainerBuilder();
        builder.RegisterAppCoreServices();
        await using var container = builder.Build();

        Type[] expected =
        [
            typeof(IAppDownloadManager),
            typeof(IAppLocalService),
            typeof(IAppSettingService),
            typeof(IArchiveService),
            typeof(IAuthService),
            typeof(IFileSystemService),
            typeof(IFileSystemPickerService),
            typeof(IJsonSerializationService),
            typeof(IMessageBoxService),
            typeof(INotificationService),
            typeof(IResourceService),
            typeof(ITelemetryService),
            typeof(IUpdateService),
            typeof(IVdfSerializationService),
            typeof(AuthState),
            typeof(Config),
            typeof(MuseDashConfig),
            typeof(MuseDash2Config)
        ];

        using var _ = Assert.Multiple();
        foreach (var t in expected)
        {
            await Assert.That(container.IsRegistered(t)).IsTrue();
        }
    }
}