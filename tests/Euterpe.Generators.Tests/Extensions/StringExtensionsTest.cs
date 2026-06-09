using Euterpe.Generators.Extensions;

namespace Euterpe.Generators.Tests.Extensions;

[TestSubject(typeof(StringExtensions))]
public sealed class StringExtensionsTest
{
    [Test]
    [Arguments("plain text", "plain text")]
    [Arguments("a & b", "a &amp; b")]
    [Arguments("<tag>", "&lt;tag&gt;")]
    [Arguments("a < b > c & d", "a &lt; b &gt; c &amp; d")]
    [Arguments("", "")]
    public async Task EscapeXmlDoc_ReplacesXmlSpecialCharacters(string input, string expected) =>
        await Assert.That(input.EscapeXmlDoc()).IsEqualTo(expected);

    [Test]
    public async Task EscapeXmlDoc_EscapesAmpersandFirst_AvoidsDoubleEscaping() =>
        await Assert.That("&lt;".EscapeXmlDoc()).IsEqualTo("&amp;lt;");

    [Test]
    [Arguments("Identifier", "Identifier")]
    [Arguments("Has Space", "Has_Space")]
    [Arguments("dashes-and.dots", "dashes_and_dots")]
    [Arguments("digits123", "digits123")]
    [Arguments("symbols!@#$", "symbols____")]
    [Arguments("", "")]
    public async Task GetValidIdentifier_ReplacesNonAlphanumericWithUnderscore(string input, string expected) =>
        await Assert.That(input.GetValidIdentifier()).IsEqualTo(expected);
}
