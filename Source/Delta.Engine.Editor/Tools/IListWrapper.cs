using System;
using System.Collections;
using System.Collections.Generic;

namespace Delta.Engine.Editor.Hierarchy;

internal readonly struct IListWrapper<T, K> : IList<T> where T : K
{
    private readonly IList<K> _list;

    internal IListWrapper(IList<K> list) => _list = list;

    public int Count => _list.Count;
    public bool IsReadOnly => _list.IsReadOnly;
    public T this[int index]
    {
        get => _list[index] is T value
            ? value
            : throw new InvalidCastException($"List item at index {index} is not {typeof(T).Name}.");
        set => _list[index] = value;
    }

    public int IndexOf(T item) => _list.IndexOf(item);
    public void Insert(int index, T item) => _list.Insert(index, item);
    public void RemoveAt(int index) => _list.RemoveAt(index);
    public void Add(T item) => _list.Add(item);
    public void Clear() => _list.Clear();
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex)
    {
        int count = _list.Count;
        for (int i = 0; i < count; i++)
            array[i] = _list[i] is T value
                ? value
                : throw new InvalidCastException($"List item at index {i} is not {typeof(T).Name}.");
    }

    public bool Remove(T item) => _list.Remove(item);
    public IEnumerator<T> GetEnumerator() => new IEnumeratorWrapper(_list.GetEnumerator());
    IEnumerator IEnumerable.GetEnumerator() => new IEnumeratorWrapper(_list.GetEnumerator());

    private readonly struct IEnumeratorWrapper(IEnumerator<K> enumerator) : IEnumerator<T>
    {
        public readonly T Current => enumerator.Current is T value
            ? value
            : throw new InvalidCastException($"Enumerator item is not {typeof(T).Name}.");
        readonly object IEnumerator.Current => Current;
        public readonly void Dispose() => enumerator.Dispose();
        public readonly bool MoveNext() => enumerator.MoveNext();
        public readonly void Reset() => enumerator.Dispose();
    }
}
