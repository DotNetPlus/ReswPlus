using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// An array that compares equal to another one holding the same items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <remarks>
/// Everything that travels through the pipeline of an incremental generator is compared against the value the
/// previous run produced, and whatever compares unequal is recomputed. <see cref="ImmutableArray{T}"/> compares
/// by reference, so a stage returning one is recomputed on every run whatever it holds, and so is every stage
/// below it.
/// </remarks>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? _items;

    public EquatableArray(IEnumerable<T> items)
    {
        _items = items as T[] ?? items.ToArray();
    }

    /// <summary>
    /// Gets the number of items.
    /// </summary>
    public int Count => _items?.Length ?? 0;

    /// <summary>
    /// Gets the item at the given position.
    /// </summary>
    public T this[int index] => _items![index];

    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> other)
    {
        if (Count != other.Count)
        {
            return false;
        }

        for (var i = 0; i < Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(this[i], other[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;

        for (var i = 0; i < Count; i++)
        {
            hash = (hash * 31) + (this[i]?.GetHashCode() ?? 0);
        }

        return hash;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
