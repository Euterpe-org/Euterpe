namespace Euterpe.Tests.Charts;

[Category("ManifestMapTests")]
[TestSubject(typeof(ManifestMap))]
public sealed class ManifestMapTest
{
    [Test]
    [Arguments("8", 8)]
    [Arguments("12", 12)]
    [Arguments("9.5", 9.5)]
    [Arguments("11+", 11.5)]
    [Arguments("", -1)]
    [Arguments("+", -1)]
    [Arguments("abc", -1)]
    public async Task RatingValue_Rating_ParsesToSortValue(string rating, double expected)
    {
        var map = new ManifestMap { Rating = rating, Charters = [] };

        await Assert.That(map.RatingValue).IsEqualTo(expected);
    }
}
