using System.Collections;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace MuseDashModTools.Common.Collections;

/// <summary>
///     A bidirectional dictionary that can retrieve a value by key or a key by value.
/// </summary>
/// <typeparam name="T1">Type of Key</typeparam>
/// <typeparam name="T2">Type of Value</typeparam>
[CollectionBuilder(typeof(BiDictionary), nameof(BiDictionary.Create))]
public sealed class BiDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward;
    private readonly Dictionary<T2, T1> _reverse;

    public T2 this[T1 key]
    {
        get => _forward[key];
        set => _forward[key] = value;
    }

    public T1 this[T2 key]
    {
        get => _reverse[key];
        set => _reverse[key] = value;
    }

    /// <summary>
    ///     Gets the number of elements stored in this BiDictionary.
    /// </summary>
    public int Count => _forward.Count;

    /// <summary>
    ///     Initializes an empty BiDictionary using the default equality comparer.
    /// </summary>
    public BiDictionary()
    {
        _forward = [];
        _reverse = [];
    }

    /// <summary>
    ///     Returns an enumerator of (Key, Value) pairs for iteration.
    /// </summary>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => _forward.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Adds a (key, value) pair to the dictionary. Throws an exception if the same key or value already exists.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(T1 key, T2 value)
    {
        if (_forward.ContainsKey(key))
        {
            throw new ArgumentException("An element with the same key already exists.");
        }

        if (_reverse.ContainsKey(value))
        {
            throw new ArgumentException("An element with the same value already exists.");
        }

        _forward.Add(key, value);
        _reverse.Add(value, key);
    }

    /// <summary>
    ///     Removes the entry corresponding to the specified key. Returns true if removed, otherwise false.
    /// </summary>
    public bool RemoveByKey(T1 key)
    {
        if (!_forward.Remove(key, out var value))
        {
            return false;
        }

        _reverse.Remove(value);
        return true;
    }

    /// <summary>
    ///     Removes the entry corresponding to the specified value. Returns true if removed, otherwise false.
    /// </summary>
    public bool RemoveByValue(T2 value)
    {
        if (!_reverse.Remove(value, out var key))
        {
            return false;
        }

        _forward.Remove(key);
        return true;
    }

    /// <summary>
    ///     Attempts to get the value associated with the specified key. Returns false if not found.
    /// </summary>
    public bool TryGetValue(T1 key, out T2? value) => _forward.TryGetValue(key, out value);

    /// <summary>
    ///     Attempts to get the key associated with the specified value. Returns false if not found.
    /// </summary>
    public bool TryGetKey(T2 value, out T1? key) => _reverse.TryGetValue(value, out key);

    /// <summary>
    ///     Determines whether the dictionary contains the specified key.
    /// </summary>
    public bool ContainsKey(T1 key) => _forward.ContainsKey(key);

    /// <summary>
    ///     Determines whether the dictionary contains the specified value.
    /// </summary>
    public bool ContainsValue(T2 value) => _reverse.ContainsKey(value);

    /// <summary>
    ///     Clears the entire dictionary.
    /// </summary>
    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    public FrozenBiDictionary<T1, T2> ToFrozenBiDictionary() => new(_forward.ToFrozenDictionary(), _reverse.ToFrozenDictionary());
}

public static class BiDictionary
{
    public static BiDictionary<T1, T2> Create<T1, T2>(ReadOnlySpan<KeyValuePair<T1, T2>> source) where T1 : notnull where T2 : notnull
    {
        var dict = new BiDictionary<T1, T2>();
        foreach (var pair in source)
        {
            dict.Add(pair.Key, pair.Value);
        }

        return dict;
    }
}