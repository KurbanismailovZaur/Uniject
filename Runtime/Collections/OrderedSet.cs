using System;
using System.Collections;
using System.Collections.Generic;

namespace Uniject.Collections
{
    internal sealed class OrderedSet<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> _items = new();
        private readonly HashSet<T> _set = new();

        public int Count => _items.Count;

        public bool Add(T item)
        {
            if (!_set.Add(item))
                return false;

            _items.Add(item);
            return true;
        }

        public bool Contains(T item) => _set.Contains(item);

        public void RemoveLast(T expectedItem)
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Collection is empty.");

            var index = _items.Count - 1;
            var item = _items[index];

            if (!EqualityComparer<T>.Default.Equals(item, expectedItem))
                throw new InvalidOperationException($"Expected last item {expectedItem}, but found {item}.");

            _items.RemoveAt(index);
            _set.Remove(item);
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}