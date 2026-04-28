using System.Text.Json;
using Euterpe.Core.Converters;
using Semver;

namespace Euterpe.Tests;

[Category("SemVersionConverterTests")]
[TestSubject(typeof(SemVersionConverter))]
public sealed class SemVersionConverterTest
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new SemVersionConverter() }
    };

    [Test]
    [Arguments("1.0.0")]
    [Arguments("0.0.1")]
    [Arguments("2.5.10")]
    [Arguments("1.0.0-rc1")]
    [Arguments("1.0.0-alpha.1")]
    [Arguments("1.0.0+build.1")]
    [Arguments("999.999.999-rc99")]
    public async Task Roundtrip_PreservesVersionString(string versionString)
    {
        var version = SemVersion.Parse(versionString);

        var json = JsonSerializer.Serialize(version, Options);
        var deserialized = JsonSerializer.Deserialize<SemVersion>(json, Options);

        using var _ = Assert.Multiple();
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.ToString()).IsEqualTo(versionString);
    }

    [Test]
    [Arguments("1.0.0", "\"1.0.0\"")]
    [Arguments("2.5.10-rc1", "\"2.5.10-rc1\"")]
    public async Task Serialize_ProducesQuotedVersionString(string versionString, string expectedJson)
    {
        var version = SemVersion.Parse(versionString);
        var json = JsonSerializer.Serialize(version, Options);
        await Assert.That(json).IsEqualTo(expectedJson);
    }

    [Test]
    public async Task Deserialize_NullJson_ReturnsNull()
    {
        var deserialized = JsonSerializer.Deserialize<SemVersion?>("null", Options);
        await Assert.That(deserialized).IsNull();
    }

    [Test]
    public async Task Deserialize_InvalidVersionString_Throws()
    {
        Action act = () => JsonSerializer.Deserialize<SemVersion>("\"not.a.version\"", Options);
        await Assert.That(act).Throws<Exception>();
    }
}
