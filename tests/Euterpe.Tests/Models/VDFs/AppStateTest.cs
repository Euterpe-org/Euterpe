using Euterpe.Models.VDFs;

namespace Euterpe.Tests.VDFs;

[Category("AppStateTests")]
[TestSubject(typeof(AppState))]
public sealed class AppStateTest
{
    [Test]
    public async Task Defaults_AreZeroEmptyAndEmptyDictionary()
    {
        var state = new AppState();

        using var _ = Assert.Multiple();
        await Assert.That(state.Appid).IsEqualTo(0);
        await Assert.That(state.Name).IsEqualTo(string.Empty);
        await Assert.That(state.InstalledDepots).IsEmpty();
    }

    [Test]
    public async Task SettersStoreValues()
    {
        var state = new AppState
        {
            Appid = 774171,
            Name = "Muse Dash",
            InstalledDepots = new Dictionary<string, Dictionary<string, string>>
            {
                ["774172"] = new() { ["manifest"] = "abc" }
            }
        };

        using var _ = Assert.Multiple();
        await Assert.That(state.Appid).IsEqualTo(774171);
        await Assert.That(state.Name).IsEqualTo("Muse Dash");
        await Assert.That(state.InstalledDepots["774172"]["manifest"]).IsEqualTo("abc");
    }
}