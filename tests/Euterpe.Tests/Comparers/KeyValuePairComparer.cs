namespace Euterpe.Tests.Comparers;

// Constrains TKey/TValue to IEquatable so equality stays off EqualityComparer<KVP>.Default's reflection fallback (AOT-safe).
public sealed class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    where TKey : IEquatable<TKey>
    where TValue : IEquatable<TValue>
{
    public static readonly KeyValuePairComparer<TKey, TValue> Default = new();

    public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
        x.Key.Equals(y.Key) && x.Value.Equals(y.Value);

    public int GetHashCode(KeyValuePair<TKey, TValue> obj) => HashCode.Combine(obj.Key, obj.Value);
}
