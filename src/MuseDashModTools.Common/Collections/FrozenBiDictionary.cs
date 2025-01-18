using System.Collections;
using System.Collections.Frozen;

namespace MuseDashModTools.Common.Collections;

public sealed class FrozenBiDictionary<T1, T2>(FrozenDictionary<T1, T2> forward, FrozenDictionary<T2, T1> reverse)
    : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    public T2 this[T1 key] => forward[key];

    public T1 this[T2 key] => reverse[key];

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => forward.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}