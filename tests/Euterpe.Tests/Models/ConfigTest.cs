namespace Euterpe.Tests;

[Category("ConfigTests")]
[TestSubject(typeof(Config))]
public sealed class ConfigTest
{
    [Test]
    public async Task DefaultUpdateChannel_IsStable() =>
        await Assert.That(new Config { MuseDash = new MuseDashConfig() }.UpdateChannel).IsEqualTo(UpdateChannel.Stable);
}