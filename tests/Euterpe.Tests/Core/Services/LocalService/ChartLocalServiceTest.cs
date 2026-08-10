using TUnit.Mocks.Logging;

namespace Euterpe.Tests.Core;

[Category("ChartLocalServiceTests")]
[TestSubject(typeof(ChartLocalService))]
public sealed class ChartLocalServiceTest
{
    private string _chartFolder = null!;

    [Before(Test)]
    public void Setup()
    {
        _chartFolder = Path.Combine(Path.GetTempPath(), $"ChartLocalServiceTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_chartFolder);
        File.WriteAllText(Path.Combine(_chartFolder, ChartFiles.ManifestFileName), string.Empty);
    }

    [After(Test)]
    public void Cleanup() => Directory.Delete(_chartFolder, true);

    [Test]
    [Arguments(ChartSource.Online, null, true)]
    [Arguments(ChartSource.Online, 13, false)]
    [Arguments(ChartSource.Offline, null, false)]
    public async Task LoadChartFromPathAsync_SourceAndCid_EnforcesOnlineCid(
        ChartSource source,
        int? cid,
        bool expectedNull)
    {
        var serialization = IMessagePackSerializationService.Mock();
        serialization.DeserializeManifestFromFileAsync(Any<string>(), Any<CancellationToken>()).Returns(CreateManifest(cid));

        var service = new ChartLocalService
        {
            GameConfig = new MuseDashConfig { Folder = _chartFolder },
            Logger = Mock.Logger<ChartLocalService>(),
            MessagePackSerialization = serialization
        };

        var chart = await service.LoadChartFromPathAsync(_chartFolder, source);

        await Assert.That(chart is null).IsEqualTo(expectedNull);
    }

    private static Manifest CreateManifest(int? cid) =>
        new()
        {
            Schema = Manifest.CurrentSchema,
            Cid = cid,
            Meta = new ManifestMeta
            {
                Name = "Chart",
                Author = "Author",
                Scene = "Scene",
                Maps = []
            },
            Files = []
        };
}
