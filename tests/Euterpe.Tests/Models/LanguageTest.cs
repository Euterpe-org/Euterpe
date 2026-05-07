using System.Globalization;

namespace Euterpe.Tests;

[Category("LanguageTests")]
[TestSubject(typeof(Language))]
public sealed class LanguageTest
{
    [Test]
    [Arguments("en-US")]
    [Arguments("zh-Hans")]
    [Arguments("ja-JP")]
    public async Task ImplicitConversion_FromString_PopulatesNameAndDisplayName(string code)
    {
        Language language = code;

        using var _ = Assert.Multiple();
        await Assert.That(language.Name).IsEqualTo(code);
        await Assert.That(language.ToString()).StartsWith($"{code} - ");
    }

    [Test]
    public async Task ToString_FormatsAsNameDashDisplayName()
    {
        Language language = "en-US";
        var expected = $"en-US - {CultureInfo.GetCultureInfo("en-US").DisplayName}";
        await Assert.That(language.ToString()).IsEqualTo(expected);
    }
}