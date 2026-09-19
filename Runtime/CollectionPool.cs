using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Uniject
{
    public sealed class CollectionPool : IDisposable
    {
        private enum CollectionKind
        {
            List,
            Array,
            HashSet,
            Dictionary,
            Queue,
            Stack
        }

        private readonly struct BucketKey : IEquatable<BucketKey>
        {
            public readonly CollectionKind Kind;
            public readonly Type FirstType;
            public readonly Type SecondType;
            public readonly int Capacity;

            public BucketKey(CollectionKind kind, Type firstType, Type secondType, int capacity)
            {
                Kind = kind;
                FirstType = firstType;
                SecondType = secondType;
                Capacity = capacity;
            }

            public bool Equals(BucketKey other)
            {
                return Kind == other.Kind && FirstType == other.FirstType && SecondType == other.SecondType &&
                       Capacity == other.Capacity;
            }

            public override bool Equals(object obj) => obj is BucketKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Kind, FirstType, SecondType, Capacity);
        }

        private sealed class ObjectReferenceComparer : IEqualityComparer<object>
        {
            public static ObjectReferenceComparer Instance { get; } = new();

            public new bool Equals(object x, object y) => ReferenceEquals(x, y);

            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private readonly Dictionary<BucketKey, Stack<object>> _despawnedCollections = new();
        private readonly Dictionary<object, BucketKey> _spawnedCollections =
            new(ObjectReferenceComparer.Instance);

        private BucketKey _lastBucketKey;
        private Stack<object> _lastBucket;

        private bool _isDisposed;

        public List<T> SpawnList<T>(int capacity = 0)
        {
            ThrowIfDisposed();
            ValidateCapacity(capacity, nameof(capacity));

            var key = new BucketKey(CollectionKind.List, typeof(T), null, capacity);
            var cached = TakeDespawned(key);
            var collection = cached == null ? new List<T>(capacity) : (List<T>)cached;
            TrackSpawned(collection, key);
            return collection;
        }

        public void DespawnList<T>(List<T> collection)
        {
            var key = ValidateAndUntrackDespawn(collection, nameof(collection));
            collection.Clear();
            TrackDespawned(collection, key);
        }

        public T[] SpawnArray<T>(int length)
        {
            ThrowIfDisposed();
            ValidateCapacity(length, nameof(length));

            var key = new BucketKey(CollectionKind.Array, typeof(T), null, length);
            var cached = TakeDespawned(key);
            var array = cached == null ? new T[length] : (T[])cached;
            TrackSpawned(array, key);
            return array;
        }

        public void DespawnArray<T>(T[] array)
        {
            var key = ValidateAndUntrackDespawn(array, nameof(array));
            Array.Clear(array, 0, array.Length);
            TrackDespawned(array, key);
        }

        public HashSet<T> SpawnHashSet<T>(int capacity = 0)
        {
            ThrowIfDisposed();
            ValidateCapacity(capacity, nameof(capacity));

            var key = new BucketKey(CollectionKind.HashSet, typeof(T), null, capacity);
            var cached = TakeDespawned(key);
            var collection = cached == null ? new HashSet<T>(capacity) : (HashSet<T>)cached;
            TrackSpawned(collection, key);
            return collection;
        }

        public void DespawnHashSet<T>(HashSet<T> collection)
        {
            var key = ValidateAndUntrackDespawn(collection, nameof(collection));
            collection.Clear();
            TrackDespawned(collection, key);
        }

        public Dictionary<TKey, TValue> SpawnDictionary<TKey, TValue>(int capacity = 0)
        {
            ThrowIfDisposed();
            ValidateCapacity(capacity, nameof(capacity));

            var key = new BucketKey(CollectionKind.Dictionary, typeof(TKey), typeof(TValue), capacity);
            var cached = TakeDespawned(key);
            var collection = cached == null
                ? new Dictionary<TKey, TValue>(capacity)
                : (Dictionary<TKey, TValue>)cached;
            TrackSpawned(collection, key);
            return collection;
        }

        public void DespawnDictionary<TKey, TValue>(Dictionary<TKey, TValue> collection)
        {
            var key = ValidateAndUntrackDespawn(collection, nameof(collection));
            collection.Clear();
            TrackDespawned(collection, key);
        }

        public Queue<T> SpawnQueue<T>(int capacity = 0)
        {
            ThrowIfDisposed();
            ValidateCapacity(capacity, nameof(capacity));

            var key = new BucketKey(CollectionKind.Queue, typeof(T), null, capacity);
            var cached = TakeDespawned(key);
            var collection = cached == null ? new Queue<T>(capacity) : (Queue<T>)cached;
            TrackSpawned(collection, key);
            return collection;
        }

        public void DespawnQueue<T>(Queue<T> collection)
        {
            var key = ValidateAndUntrackDespawn(collection, nameof(collection));
            collection.Clear();
            TrackDespawned(collection, key);
        }

        public Stack<T> SpawnStack<T>(int capacity = 0)
        {
            ThrowIfDisposed();
            ValidateCapacity(capacity, nameof(capacity));

            var key = new BucketKey(CollectionKind.Stack, typeof(T), null, capacity);
            var cached = TakeDespawned(key);
            var collection = cached == null ? new Stack<T>(capacity) : (Stack<T>)cached;
            TrackSpawned(collection, key);
            return collection;
        }

        public void DespawnStack<T>(Stack<T> collection)
        {
            var key = ValidateAndUntrackDespawn(collection, nameof(collection));
            collection.Clear();
            TrackDespawned(collection, key);
        }

        public void Clear()
        {
            ThrowIfDisposed();

            _despawnedCollections.Clear();
            _lastBucket = null;
            _lastBucketKey = default;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _despawnedCollections.Clear();
            _spawnedCollections.Clear();

            _lastBucket = null;
            _lastBucketKey = default;

            _isDisposed = true;
        }

        private Stack<object> GetDespawnedBucket(BucketKey key, bool createIfMissing)
        {
            if (_lastBucket != null && _lastBucketKey.Equals(key))
                return _lastBucket;

            if (!_despawnedCollections.TryGetValue(key, out var collections))
            {
                if (!createIfMissing)
                    return null;

                collections = new Stack<object>();
                _despawnedCollections.Add(key, collections);
            }

            _lastBucketKey = key;
            _lastBucket = collections;

            return collections;
        }

        private object TakeDespawned(BucketKey key)
        {
            var collections = GetDespawnedBucket(key, createIfMissing: false);

            if (collections == null || collections.Count == 0)
                return null;

            return collections.Pop();
        }

        private void TrackSpawned(object collection, BucketKey key)
        {
            if (!_spawnedCollections.TryAdd(collection, key))
                throw new InvalidOperationException("Collection is already spawned.");
        }

        private BucketKey ValidateAndUntrackDespawn<TCollection>(TCollection collection,
            string parameterName)
            where TCollection : class
        {
            ThrowIfDisposed();

            if (collection == null)
                throw new ArgumentNullException(parameterName);

            if (collection.GetType() != typeof(TCollection))
            {
                throw new InvalidOperationException("Collection is not spawned by this pool or has an incompatible type.");
            }

            if (!_spawnedCollections.Remove(collection, out var key))
            {
                throw new InvalidOperationException("Collection is not spawned by this pool or has an incompatible type.");
            }

            return key;
        }

        private void TrackDespawned(object collection, BucketKey key)
        {
            var collections = GetDespawnedBucket(key, createIfMissing: true);
            collections.Push(collection);
        }

        private static void ValidateCapacity(int capacity, string parameterName)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Capacity can not be less than zero.");
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CollectionPool));
        }
    }
}
