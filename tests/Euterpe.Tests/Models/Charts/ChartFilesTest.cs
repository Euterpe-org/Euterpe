namespace Euterpe.Tests.Models.Charts;

[Category("ChartFilesTests")]
[TestSubject(typeof(ChartFiles))]
public sealed class ChartFilesTest
{
    [Test]
    [Arguments("manifest.epk")]
    [Arguments("music.ogg")]
    [Arguments("demo.ogg")]
    [Arguments("video.mp4")]
    [Arguments("cover.webp")]
    [Arguments("cover.gif")]
    [Arguments("cover.png")]
    [Arguments("map1.bms")]
    [Arguments("map4.bms")]
    [Arguments("map3.talk")]
    public async Task IsChartFile_KnownOrRetiredChartFile_ReturnsTrue(string fileName) =>
        await Assert.That(ChartFiles.IsChartFile(fileName)).IsTrue();

    [Test]
    [Arguments("thumbs.db")]
    [Arguments("music.ogg.tmp")]
    [Arguments("notes.txt")]
    [Arguments("song.mp3")]
    [Arguments("map.bms")]
    [Arguments("mapx.bms")]
    [Arguments("covers.png")]
    public async Task IsChartFile_ForeignFile_ReturnsFalse(string fileName) =>
        await Assert.That(ChartFiles.IsChartFile(fileName)).IsFalse();

    [Test]
    [Arguments("cover.webp", true)]
    [Arguments("cover.gif", true)]
    [Arguments("cover.png", true)]
    [Arguments("cover.jpg", true)]
    [Arguments("music.ogg", false)]
    [Arguments("covers.png", false)]
    public async Task IsCoverFile_StemIsCover_MatchesAnyExtension(string fileName, bool expected) =>
        await Assert.That(ChartFiles.IsCoverFile(fileName)).IsEqualTo(expected);
}
