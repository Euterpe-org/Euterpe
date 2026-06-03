using System.Reflection;
using Autofac;
using Euterpe.Abstractions;
using Euterpe.Models.Mods;
using Euterpe.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using TUnit.Mocks.Logging;

namespace Euterpe.Headless.Tests.Services;

[TestSubject(typeof(DeepLinkService))]
public sealed class DeepLinkServiceTest : HeadlessTest
{
    private static DeepLinkService NewService(
        IAuthService? auth = null,
        IDeepLinkSetup? setup = null,
        MockLogger<DeepLinkService>? logger = null,
        IModManageService? modManageService = null)
    {
        var builder = new ContainerBuilder();
        if (modManageService is not null)
        {
            builder.RegisterInstance(modManageService).As<IModManageService>();
        }

        var container = builder.Build();
        return new DeepLinkService
        {
            NavigationService = new NavigationService
            {
                Logger = NullLogger<NavigationService>.Instance
            },
            Logger = logger ?? Mock.Logger<DeepLinkService>(),
            DeepLinkSetup = setup ?? IDeepLinkSetup.Mock(),
            LazyAuthService = new Lazy<IAuthService>(() => auth ?? IAuthService.Mock()),
            GameScope = new BehaviorSubject<ILifetimeScope>(container)
        };
    }

    [Test]
    public Task HandleStartupArgs_Empty_DoesNothing() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleStartupArgs([]);

        await Assert.That(logger.Entries).IsEmpty();
    });

    [Test]
    public Task HandleStartupArgs_NonUri_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleStartupArgs(["not-a-uri-at-all"]);

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Invalid deep link");
    });

    [Test]
    public Task HandleUri_NonAbsoluteUri_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleUri("relative/path");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Invalid deep link");
    });

    [Test]
    public Task HandleUri_WrongScheme_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleUri("http://example.com/mod/install/foo");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Invalid deep link");
    });

    [Test]
    public Task HandleUri_LogsReceivedAtInfoLevel() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleUri("not-a-uri");

        var info = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Information);
        using var _ = Assert.Multiple();
        await Assert.That(info).IsNotNull();
        await Assert.That(info!.Message).Contains("Deep link received");
        await Assert.That(info.Message).Contains("not-a-uri");
    });

    // Private HandleAuthCallbackAsync tests (avoid the ActivateMainWindow + IocContainer dependencies
    // sitting between the public HandleUri entry point and the auth dispatch).

    private static Task InvokeAuthCallback(DeepLinkService service, string query) =>
        (Task)typeof(DeepLinkService)
            .GetMethod("HandleAuthCallbackAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(service, [query])!;

    [Test]
    public async Task HandleAuthCallback_WithValidCode_CallsCompleteLogin()
    {
        var auth = IAuthService.Mock();
        var service = NewService(auth);

        await InvokeAuthCallback(service, "code=abc123&state=xyz");

        using var _ = Assert.Multiple();
        auth.CompleteLoginAsync("abc123").WasCalled(Times.Once);
        auth.LoginAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleAuthCallback_MissingCode_FallsBackToLogin()
    {
        var auth = IAuthService.Mock();
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(auth, logger: logger);

        await InvokeAuthCallback(service, "state=xyz");

        using var _ = Assert.Multiple();
        auth.CompleteLoginAsync(Any<string>()).WasCalled(Times.Never);
        auth.LoginAsync().WasCalled(Times.Once);
        await Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("missing code"))).IsTrue();
    }

    [Test]
    public async Task HandleAuthCallback_EmptyQuery_FallsBackToLogin()
    {
        var auth = IAuthService.Mock();
        var service = NewService(auth);

        await InvokeAuthCallback(service, "");

        using var _ = Assert.Multiple();
        auth.CompleteLoginAsync(Any<string>()).WasCalled(Times.Never);
        auth.LoginAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task HandleAuthCallback_CompleteLoginThrows_RetriesLogin()
    {
        var auth = IAuthService.Mock();
        auth.CompleteLoginAsync(Any<string>()).Throws(new InvalidOperationException("boom"));
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(auth, logger: logger);

        await InvokeAuthCallback(service, "code=abc123");

        using var _ = Assert.Multiple();
        auth.CompleteLoginAsync("abc123").WasCalled(Times.Once);
        auth.LoginAsync().WasCalled(Times.Once);
        await Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Error && e.Message.Contains("Auth callback failed"))).IsTrue();
    }

    // Private HandleModActionAsync / HandleChartActionAsync tests (bypass the NavigationService.Ready
    // gate + ActivateMainWindow sitting between HandleUri and the per-domain dispatch).

    private static Task InvokeModAction(DeepLinkService service, string path) =>
        (Task)typeof(DeepLinkService)
            .GetMethod("HandleModActionAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(service, [path])!;

    private static Task InvokeChartAction(DeepLinkService service, string path) =>
        (Task)typeof(DeepLinkService)
            .GetMethod("HandleChartActionAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(service, [path])!;

    [Test]
    public async Task HandleModAction_Update_WithoutName_UpdatesAllMods()
    {
        var mods = IModManageService.Mock();
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update");

        using var _ = Assert.Multiple();
        mods.UpdateAllModsAsync().WasCalled(Times.Once);
        mods.UpdateModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleModAction_Update_NamedInstalledMod_UpdatesThatMod()
    {
        var mods = IModManageService.Mock();
        var installed = new ModDto { Name = "Euterpe", FileNameWithoutExtension = "Euterpe" };
        mods.FindModByName("Euterpe").Returns(installed);
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update/Euterpe");

        using var _ = Assert.Multiple();
        mods.UpdateModAsync(installed).WasCalled(Times.Once);
        mods.UpdateAllModsAsync().WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleModAction_Update_NamedNotInstalledMod_DoesNotUpdate()
    {
        var mods = IModManageService.Mock();
        var notInstalled = new ModDto { Name = "Euterpe" };
        mods.FindModByName("Euterpe").Returns(notInstalled);
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update/Euterpe");

        mods.UpdateModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleChartAction_Convert_LogsPlaceholderWithoutWarning()
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        await InvokeChartAction(service, "convert");

        using var _ = Assert.Multiple();
        await Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Information && e.Message.Contains("Chart convert"))).IsTrue();
        await Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning)).IsFalse();
    }

    [Test]
    public async Task HandleChartAction_UnknownPath_LogsWarning()
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        await InvokeChartAction(service, "bogus");

        await Assert.That(logger.Entries.Any(e => e.LogLevel == LogLevel.Warning && e.Message.Contains("Unknown chart deep link"))).IsTrue();
    }
}