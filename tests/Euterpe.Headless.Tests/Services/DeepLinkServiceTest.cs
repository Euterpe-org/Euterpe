using System.Runtime.CompilerServices;
using Autofac;
using Euterpe.Abstractions;
using Euterpe.Models.Migrations;
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
        ISystemAssociationSetup? setup = null,
        MockLogger<DeepLinkService>? logger = null,
        IModManageService? modManageService = null,
        IChartManageService? chartManageService = null)
    {
        var builder = new ContainerBuilder();
        if (modManageService is not null)
        {
            builder.RegisterInstance(modManageService).As<IModManageService>();
        }

        if (chartManageService is not null)
        {
            builder.RegisterInstance(chartManageService).As<IChartManageService>();
        }

        var container = builder.Build();
        return new DeepLinkService
        {
            NavigationService = new NavigationService
            {
                Logger = NullLogger<NavigationService>.Instance
            },
            Logger = logger ?? Mock.Logger<DeepLinkService>(),
            AssociationSetup = setup ?? ISystemAssociationSetup.Mock(),
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
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_NonAbsoluteUri_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleActivation("relative/path");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_WrongScheme_LogsWarning() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleActivation("http://example.com/mod/install/foo");

        var warning = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Warning);
        using var _ = Assert.Multiple();
        await Assert.That(warning).IsNotNull();
        await Assert.That(warning!.Message).Contains("Unhandled activation");
    });

    [Test]
    public Task HandleActivation_LogsReceivedAtInfoLevel() => RunOnUI(async () =>
    {
        var logger = Mock.Logger<DeepLinkService>();
        var service = NewService(logger: logger);

        service.HandleActivation("not-a-uri");

        var info = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Information);
        using var _ = Assert.Multiple();
        await Assert.That(info).IsNotNull();
        await Assert.That(info!.Message).Contains("Activation received");
        await Assert.That(info.Message).Contains("not-a-uri");
    });

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetEpkPath")]
    private static extern string? InvokeGetEpkPath(DeepLinkService? service, string argument);

    [Test]
    public async Task GetEpkPath_RelativeEpkPath_ReturnsArgument()
    {
        await Assert.That(InvokeGetEpkPath(null, "foo.epk")).IsEqualTo("foo.epk");
    }

    [Test]
    public async Task GetEpkPath_EpkFileUri_ReturnsLocalPathEndingInExtension()
    {
        var result = InvokeGetEpkPath(null, "file:///charts/foo.epk");

        using var _ = Assert.Multiple();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!).EndsWith(".epk");
    }

    [Test]
    [Arguments("euterpe://mod/install/foo")]
    [Arguments("http://example.com/charts/foo.epk")]
    [Arguments("file:///charts/foo.txt")]
    [Arguments("not-a-uri")]
    public async Task GetEpkPath_NonEpk_ReturnsNull(string argument)
    {
        await Assert.That(InvokeGetEpkPath(null, argument)).IsNull();
    }

    // Private HandleModActionAsync / HandleChartActionAsync tests (bypass the NavigationService.Ready
    // gate + ActivateMainWindow sitting between HandleActivation and the per-domain dispatch).

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleModActionAsync")]
    private static extern Task InvokeModAction(DeepLinkService service, string path);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleChartActionAsync")]
    private static extern Task InvokeChartAction(DeepLinkService service, string path);

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
        var installed = new ModDto { Name = "Euterpe", FileNameWithoutExtension = "Euterpe", State = ModState.Outdated };
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
    public async Task HandleModAction_Update_NamedNotOutdatedMod_DoesNotUpdate()
    {
        var mods = IModManageService.Mock();
        var upToDate = new ModDto { Name = "Euterpe", FileNameWithoutExtension = "Euterpe", State = ModState.Normal };
        mods.FindModByName("Euterpe").Returns(upToDate);
        var service = NewService(modManageService: mods);

        await InvokeModAction(service, "update/Euterpe");

        mods.UpdateModAsync(Any<ModDto>()).WasCalled(Times.Never);
    }

    [Test]
    public async Task HandleChartAction_Convert_MigratesCustomAlbums()
    {
        var logger = Mock.Logger<DeepLinkService>();
        var charts = IChartManageService.Mock();
        var service = NewService(logger: logger, chartManageService: charts);

        await InvokeChartAction(service, "convert");

        using var _ = Assert.Multiple();
        charts.MigrateCustomAlbumsAsync(Any<IProgress<MigrationProgress>?>(), Any<CancellationToken>()).WasCalled(Times.Once);
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
