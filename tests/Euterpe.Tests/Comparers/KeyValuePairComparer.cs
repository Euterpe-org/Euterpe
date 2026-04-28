namespace Euterpe.Tests.Comparers;

/// <summary>
///     AOT-safe equality comparer for <see cref="KeyValuePair{TKey,TValue}" /> entries.
///     <see cref="KeyValuePair{TKey,TValue}" /> does not implement <see cref="IEquatable{T}" />,
///     so the runtime <see cref="EqualityComparer{T}.Default" /> falls back to reflection-based
///     comparison (the "inefficient runtime-provided implementation" warning). This comparer
///     constrains <typeparamref name="TKey" /> and <typeparamref name="TValue" /> to
///     <see cref="IEquatable{T}" /> so the comparison stays on the fast path and AOT-friendly.
/// </summary>
public sealed class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    where TKey : IEquatable<TKey>
    where TValue : IEquatable<TValue>
{
    public static readonly KeyValuePairComparer<TKey, TValue> Default = new();

    public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
        x.Key.Equals(y.Key) && x.Value.Equals(y.Value);

    public int GetHashCode(KeyValuePair<TKey, TValue> obj) => HashCode.Combine(obj.Key, obj.Value);
}