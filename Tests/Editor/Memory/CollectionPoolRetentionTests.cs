using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Uniject.Tests.Memory
{
    [Category("MemoryRetention")]
    public class CollectionPoolRetentionTests
    {
        public enum CollectionKind { List, Array, HashSet, Dictionary, Queue, Stack }

        private sealed class References
        {
            public WeakReference Collection;
            public WeakReference[] Elements;
        }

        [Test]
        public void Despawn_DoesNotRetainElements([Values] CollectionKind kind)
        {
            using var pool = new CollectionPool();
            var references = Fill(pool, kind, true);

            MemoryRetentionAssert.Collected(pool, references.Elements);
            // Keeping reusable storage is intentional; retaining its old elements is not.
            MemoryRetentionAssert.Retained(pool, references.Collection);
        }

        [Test]
        public void Clear_ReleasesReturnedCollectionIncludingLastBucket([Values] CollectionKind kind)
        {
            using var pool = new CollectionPool();
            var references = Fill(pool, kind, true);
            MemoryRetentionAssert.Retained(pool, references.Collection);

            pool.Clear();

            MemoryRetentionAssert.Collected(pool, references.Collection);
        }

        [Test]
        public void Clear_KeepsBorrowedCollectionTrackedUntilDispose([Values] CollectionKind kind)
        {
            using var pool = new CollectionPool();
            var references = Fill(pool, kind, false);

            pool.Clear();

            MemoryRetentionAssert.Retained(pool, references.Collection);
            MemoryRetentionAssert.Retained(pool, references.Elements);
            pool.Dispose();
            MemoryRetentionAssert.Collected(pool, references.Collection);
            MemoryRetentionAssert.Collected(pool, references.Elements);
        }

        [Test]
        public void Dispose_ReleasesBothBorrowedAndReturnedCollections([Values] CollectionKind kind)
        {
            using var pool = new CollectionPool();
            var borrowed = Fill(pool, kind, false);
            var returned = Fill(pool, kind, true);

            pool.Dispose();

            MemoryRetentionAssert.Collected(pool, borrowed.Collection, returned.Collection);
            MemoryRetentionAssert.Collected(pool, borrowed.Elements);
            MemoryRetentionAssert.Collected(pool, returned.Elements);
        }

        [Test]
        public void Clear_ReleasesEveryCapacityBucket()
        {
            using var pool = new CollectionPool();
            var references = new WeakReference[16];
            for (var i = 0; i < references.Length; i++)
                references[i] = ReturnList(pool, i * 16);

            pool.Clear();

            MemoryRetentionAssert.Collected(pool, references);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ReturnList(CollectionPool pool, int capacity)
        {
            var list = pool.SpawnList<object>(capacity);
            pool.DespawnList(list);
            return new WeakReference(list);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static References Fill(CollectionPool pool, CollectionKind kind, bool despawn)
        {
            var element = new object();
            var references = new References { Elements = new[] { new WeakReference(element) } };
            object collection;
            switch (kind)
            {
                case CollectionKind.List:
                    var list = pool.SpawnList<object>(16);
                    list.Add(element);
                    collection = list;
                    if (despawn) pool.DespawnList(list);
                    break;
                case CollectionKind.Array:
                    var array = pool.SpawnArray<object>(16);
                    array[0] = element;
                    array[array.Length - 1] = element;
                    collection = array;
                    if (despawn) pool.DespawnArray(array);
                    break;
                case CollectionKind.HashSet:
                    var hashSet = pool.SpawnHashSet<object>(16);
                    hashSet.Add(element);
                    collection = hashSet;
                    if (despawn) pool.DespawnHashSet(hashSet);
                    break;
                case CollectionKind.Dictionary:
                    var dictionary = pool.SpawnDictionary<object, object>(16);
                    var value = new object();
                    dictionary.Add(element, value);
                    references.Elements = new[] { new WeakReference(element), new WeakReference(value) };
                    collection = dictionary;
                    if (despawn) pool.DespawnDictionary(dictionary);
                    break;
                case CollectionKind.Queue:
                    var queue = pool.SpawnQueue<object>(16);
                    queue.Enqueue(element);
                    collection = queue;
                    if (despawn) pool.DespawnQueue(queue);
                    break;
                case CollectionKind.Stack:
                    var stack = pool.SpawnStack<object>(16);
                    stack.Push(element);
                    collection = stack;
                    if (despawn) pool.DespawnStack(stack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            references.Collection = new WeakReference(collection);
            return references;
        }
    }
}
