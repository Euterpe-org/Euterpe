using Euterpe.Shared.Extensions;

namespace Euterpe.Tests;

[Category("ParallelExtensionTests")]
[TestSubject(typeof(ParallelExtension))]
public sealed class ParallelExtensionTest
{
    [Test]
    public async Task WhenAllAsync_EmptyArray_ReturnsEmptyArray()
    {
        var input = Array.Empty<int>();

        var result = await input.WhenAllAsync(static i => Task.FromResult<string?>(i.ToString()));

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task WhenAllAsync_AppliesFunctionToEveryElement()
    {
        var input = new[] { 1, 2, 3, 4, 5 };

        var result = await input.WhenAllAsync(static i => Task.FromResult<string?>($"item-{i}"));
        var sorted = result.Select(x => x!).Order(StringComparer.Ordinal).ToArray();

        await Assert.That(sorted).IsEquivalentTo(["item-1", "item-2", "item-3", "item-4", "item-5"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task WhenAllAsync_PreservesNullResults()
    {
        var input = new[] { 1, 2, 3 };

        var result = await input.WhenAllAsync(static i => Task.FromResult<string?>(i % 2 is 0 ? null : i.ToString()));

        using var _ = Assert.Multiple();
        await Assert.That(result.Length).IsEqualTo(3);
        await Assert.That(result.Count(x => x is null)).IsEqualTo(1);
        var nonNull = result.OfType<string>().Order(StringComparer.Ordinal).ToArray();
        await Assert.That(nonNull).IsEquivalentTo(["1", "3"], EqualityComparer<string>.Default, CollectionOrdering.Matching);
    }

    [Test]
    public async Task WhenAllAsync_PropagatesException()
    {
        var input = new[] { 1, 2, 3 };

        Func<Task> act = () => input.WhenAllAsync(static i =>
            i is 2 ? Task.FromException<string?>(new InvalidOperationException("boom")) : Task.FromResult<string?>(i.ToString()));

        await Assert.That(act).Throws<InvalidOperationException>().WithMessage("boom");
    }
}