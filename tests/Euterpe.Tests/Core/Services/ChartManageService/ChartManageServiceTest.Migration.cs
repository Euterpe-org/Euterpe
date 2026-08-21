using System.Collections.Concurrent;
using Euterpe.Models.Charts.CustomAlbums;
using Euterpe.Models.Migrations;

namespace Euterpe.Tests.Core;

public sealed partial class ChartManageServiceTest
{
    [Test]
    public async Task MigrateCustomAlbumFilesAsync_SelectedFiles_MigratesAndDeletesSources()
    {
        var fileSystem = IFileSystemService.Mock();
        var migration = new RecordingMigrationService();
        var service = CreateService(new FakeChartLocalService(), fileSystemService: fileSystem, migrationService: migration);

        var processed = await service.MigrateCustomAlbumFilesAsync(["/imports/one.mdm", "/imports/two.MDM"]);

        using var _ = Assert.Multiple();
        await Assert.That(processed).IsEqualTo(2);
        await Assert.That(migration.Sources).IsEquivalentTo(
        [
            new CustomAlbumSource("/imports/one.mdm", false),
            new CustomAlbumSource("/imports/two.MDM", false)
        ], EqualityComparer<CustomAlbumSource>.Default);
        fileSystem.TryDeleteFile("/imports/one.mdm").WasCalled(Times.Once);
        fileSystem.TryDeleteFile("/imports/two.MDM").WasCalled(Times.Once);
    }

    private sealed class RecordingMigrationService : IMigrationService
    {
        public ConcurrentBag<CustomAlbumSource> Sources { get; } = [];

        public Task<MigrationResult> MigrateCustomAlbumAsync(CustomAlbumSource source, CancellationToken cancellationToken = default)
        {
            Sources.Add(source);
            return Task.FromResult(new MigrationResult(MigrationOutcome.Migrated, $"/offline/{source.Name}"));
        }
    }
}
