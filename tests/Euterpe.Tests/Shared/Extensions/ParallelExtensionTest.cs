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
        var sorted = result.OrderBy(x => x, StringComparer.Ordinal).ToArray();

        await Assert.That(sorted.SequenceEqual(["item-1", "item-2", "item-3", "item-4", "item-5"], StringComparer.Ordinal)).IsTrue();
    }

    [Test]
    public async Task WhenAllAsync_PreservesNullResults()
    {
        var input = new[] { 1, 2, 3 };

        var result = await input.WhenAllAsync(static i => Task.FromResult<string?>(i % 2 == 0 ? null : i.ToString()));

        using var _ = Assert.Multiple();
        await Assert.That(result.Length).IsEqualTo(3);
        await Assert.That(result.Count(x => x is null)).IsEqualTo(1);
        var nonNull = result.Where(x => x is not null).Select(x => x!).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        await Assert.That(nonNull.SequenceEqual(["1", "3"], StringComparer.Ordinal)).IsTrue();
    }

    [Test]
    [Timeout(5_000)]
    public async Task WhenAllAsync_PropagatesException(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var input = new[] { 1, 2, 3 };

        Func<Task> act = () => input.WhenAllAsync<int, string>(static i =>
            i == 2 ? Task.FromException<string?>(new InvalidOperationException("boom")) : Task.FromResult<string?>(i.ToString()));

        await Assert.That(act).Throws<InvalidOperationException>().WithMessage("boom");
    }
}
