namespace Euterpe.Tests.Charts;

[Category("ChartDtoTests")]
[TestSubject(typeof(ChartDto))]
public sealed class ChartDtoTest
{
    private static ChartDto CreateChart(
        int? cid = null,
        ManifestUploader? uploader = null,
        Dictionary<string, ManifestMap>? maps = null,
        Dictionary<string, ManifestFileEntry>? files = null,
        ChartSource source = ChartSource.Offline,
        int bpm = 120,
        int? bpmMin = null,
        int? bpmMax = null) =>
        new()
        {
            FolderPath = "/charts/folder",
            FolderName = "folder",
            Source = source,
            Manifest = new Manifest
            {
                Schema = Manifest.CurrentSchema,
                Cid = cid,
                Meta = new ManifestMeta
                {
                    Name = "song",
                    Author = "author",
                    Scene = "scene",
                    Bpm = bpm,
                    BpmMin = bpmMin,
                    BpmMax = bpmMax,
                    Uploader = uploader,
                    Maps = maps ?? new()
                },
                Files = files ?? new()
            }
        };

    private static ManifestMap CreateMap(string rating, params string[] charters) =>
        new() { Rating = rating, Charters = charters };

    [Test]
    [Arguments(512L, "0.5 KB")]
    [Arguments(5L * (1 << 20), "5 MB")]
    [Arguments(3L * (1 << 30) / 2, "1.5 GB")]
    public async Task SizeDisplay_TotalSize_FormatsBinaryUnits(long size, string expected)
    {
        var chart = CreateChart(files: new() { ["music.ogg"] = new ManifestFileEntry { Size = size } });

        await Assert.That(chart.SizeDisplay).IsEqualTo(expected);
    }

    [Test]
    public async Task SizeBytes_MultipleFiles_Sums()
    {
        var chart = CreateChart(files: new()
        {
            ["music.ogg"] = new ManifestFileEntry { Size = 300 },
            ["map1.bms"] = new ManifestFileEntry { Size = 212 }
        });

        await Assert.That(chart.SizeBytes).IsEqualTo(512);
    }

    [Test]
    public async Task BpmDisplay_DifferentMinMax_ShowsRange()
    {
        var chart = CreateChart(bpmMin: 120, bpmMax: 140);

        await Assert.That(chart.BpmDisplay).IsEqualTo("120–140");
    }

    [Test]
    public async Task BpmDisplay_NoRange_ShowsBpm()
    {
        var chart = CreateChart(bpm: 120);

        await Assert.That(chart.BpmDisplay).IsEqualTo("120");
    }

    [Test]
    public async Task CharterDisplay_DuplicateCharters_DedupesCaseInsensitive()
    {
        var chart = CreateChart(maps: new()
        {
            ["map1"] = CreateMap("8", "Alice", "Bob"),
            ["map2"] = CreateMap("10", "alice")
        });

        await Assert.That(chart.CharterDisplay).IsEqualTo("Alice, Bob");
    }

    [Test]
    public async Task MaxRating_MultipleMaps_PicksHighest()
    {
        var chart = CreateChart(maps: new()
        {
            ["map1"] = CreateMap("8"),
            ["map2"] = CreateMap("11")
        });

        await Assert.That(chart.MaxRating).IsEqualTo(11);
    }

    [Test]
    public async Task DetailUrl_WithCid_PointsToChartPage()
    {
        var chart = CreateChart(cid: 123);

        await Assert.That(chart.DetailUrl).IsEqualTo("https://euterpe-org.com/charts/123");
    }

    [Test]
    public async Task DetailUrl_WithoutCid_IsNull()
    {
        var chart = CreateChart();

        await Assert.That(chart.DetailUrl).IsNull();
    }

    [Test]
    public async Task UploaderPageUrl_WithUploader_PointsToUserCharts()
    {
        var chart = CreateChart(uploader: new ManifestUploader { Uid = 7, Nickname = "uploader" });

        await Assert.That(chart.UploaderPageUrl).IsEqualTo("https://euterpe-org.com/users/7?tab=charts");
    }

    [Test]
    public async Task UploaderPageUrl_WithoutUploader_IsNull()
    {
        var chart = CreateChart();

        await Assert.That(chart.UploaderPageUrl).IsNull();
    }

    [Test]
    [Arguments(ChartSource.Online, true)]
    [Arguments(ChartSource.Offline, false)]
    public async Task IsOnline_Source_MatchesOnline(ChartSource source, bool expected)
    {
        var chart = CreateChart(source: source);

        await Assert.That(chart.IsOnline).IsEqualTo(expected);
    }
}
