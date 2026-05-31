using System.Security.Cryptography;
using System.Text;
using Euterpe.Core.Extensions;
using Euterpe.Core.Utils;

namespace Euterpe.Tests;

[Category("PkcePairTests")]
[TestSubject(typeof(PkcePair))]
public sealed class PkcePairTest
{
    [Test]
    public async Task Generate_ChallengeIsBase64UrlSha256OfVerifier()
    {
        var pair = PkcePair.Generate();

        var expectedChallenge = SHA256.HashData(Encoding.ASCII.GetBytes(pair.Verifier)).ToBase64Url();
        await Assert.That(pair.Challenge).IsEqualTo(expectedChallenge);
    }

    [Test]
    [Arguments("=")]
    [Arguments("+")]
    [Arguments("/")]
    public async Task Generate_ProducesUrlSafeUnpaddedValues(string forbidden)
    {
        var pair = PkcePair.Generate();

        using var _ = Assert.Multiple();
        await Assert.That(pair.Verifier).DoesNotContain(forbidden);
        await Assert.That(pair.Challenge).DoesNotContain(forbidden);
    }

    [Test]
    public async Task Generate_ProducesDistinctVerifiersAcrossCalls()
    {
        var first = PkcePair.Generate();
        var second = PkcePair.Generate();

        await Assert.That(first.Verifier).IsNotEqualTo(second.Verifier);
    }
}