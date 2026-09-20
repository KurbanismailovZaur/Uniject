using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Uniject.Tests.Performance.Memory
{
    public class CollectionPoolMemoryTests
    {
        public enum CollectionKind
        {
            List,
            Array,
            HashSet,
            Dictionary,
            Queue,
            Stack
        }

        [Test, Performance]
        public void Spawn_FirstCollection(
            [Values] CollectionKind kind, [Values(false, true)] bool referenceElements,
            [Values(0, 16, 256)] int capacity)
        {
            WithElementType(referenceElements, capacity, (values, adapter) =>
            {
                CollectionPool pool = null;
                AllocationMeasurement.Run("CollectionPool.FirstSpawn", () => adapter.Spawn(pool, kind, capacity),
                    setUp: () => pool = new CollectionPool(), cleanUp: () => pool.Dispose(), iterations: 1);
            });
        }

        [Test, Performance]
        public void Despawn_FirstBucket(
            [Values] CollectionKind kind, [Values(false, true)] bool referenceElements)
        {
            const int count = 16;
            WithElementType(referenceElements, count, (values, adapter) =>
            {
                CollectionPool pool = null;
                object collection = null;
                AllocationMeasurement.Run("CollectionPool.FirstDespawn", () => adapter.Despawn(pool, kind, collection),
                    setUp: () =>
                    {
                        pool = new CollectionPool();
                        collection = adapter.Spawn(pool, kind, count);
                        adapter.Fill(kind, collection, values);
                    },
                    cleanUp: () =>
                    {
                        collection = null;
                        pool.Dispose();
                    }, iterations: 1);
            });
        }

        [Test, Performance]
        public void SpawnFillDespawn_WarmCollection(
            [Values] CollectionKind kind, [Values(false, true)] bool referenceElements,
            [Values(0, 16, 256)] int count)
        {
            WithElementType(referenceElements, count, (values, adapter) =>
            {
                using var pool = new CollectionPool();
                Action cycle = () =>
                {
                    var collection = adapter.Spawn(pool, kind, count);
                    adapter.Fill(kind, collection, values);
                    adapter.Despawn(pool, kind, collection);
                };
                cycle();
                AllocationMeasurement.Run("CollectionPool.WarmSpawnFillDespawn", cycle, expectZero: true);
            });
        }

        [Test, Performance]
        public void Despawn_WarmFilledCollection(
            [Values] CollectionKind kind, [Values(false, true)] bool referenceElements)
        {
            const int count = 256;
            WithElementType(referenceElements, count, (values, adapter) =>
            {
                using var pool = new CollectionPool();
                object collection = adapter.Spawn(pool, kind, count);
                adapter.Despawn(pool, kind, collection);
                AllocationMeasurement.Run("CollectionPool.WarmDespawn", () => adapter.Despawn(pool, kind, collection),
                    setUp: () =>
                    {
                        collection = adapter.Spawn(pool, kind, count);
                        adapter.Fill(kind, collection, values);
                    }, expectZero: true);
            });
        }

        [Test, Performance]
        public void SpawnDespawn_WarmConcurrentBatch(
            [Values] CollectionKind kind, [Values(false, true)] bool referenceElements,
            [Values(1, 16, 64)] int simultaneousCount)
        {
            const int capacity = 16;
            WithElementType(referenceElements, capacity, (values, adapter) =>
            {
                using var pool = new CollectionPool();
                var collections = new object[simultaneousCount];
                Action cycle = () =>
                {
                    for (var i = 0; i < collections.Length; i++)
                    {
                        collections[i] = adapter.Spawn(pool, kind, capacity);
                        adapter.Fill(kind, collections[i], values);
                    }
                    for (var i = 0; i < collections.Length; i++)
                        adapter.Despawn(pool, kind, collections[i]);
                };
                cycle();
                AllocationMeasurement.Run("CollectionPool.WarmConcurrentSpawnFillDespawn", cycle,
                    operationsPerIteration: simultaneousCount, expectZero: true);
            });
        }

        [Test, Performance]
        public void Spawn_FirstConcurrentPeak([Values] CollectionKind kind, [Values(16, 64)] int simultaneousCount)
        {
            CollectionPool pool = null;
            var adapter = new CollectionAdapter<object>();
            AllocationMeasurement.Run("CollectionPool.FirstConcurrentPeak", () =>
            {
                for (var i = 0; i < simultaneousCount; i++)
                    adapter.Spawn(pool, kind, 16);
            },
                setUp: () =>
                {
                    pool = new CollectionPool();
                    adapter.Despawn(pool, kind, adapter.Spawn(pool, kind, 16));
                }, cleanUp: () => pool.Dispose(), iterations: 1, operationsPerIteration: simultaneousCount);
        }

        [Test, Performance]
        public void SpawnDespawn_AlternatingWarmBuckets([Values] CollectionKind kind)
        {
            using var pool = new CollectionPool();
            var adapter = new CollectionAdapter<object>();
            var capacities = new[] { 0, 16, 256 };
            Action cycle = () =>
            {
                for (var i = 0; i < capacities.Length; i++)
                    adapter.Despawn(pool, kind, adapter.Spawn(pool, kind, capacities[i]));
            };
            cycle();
            AllocationMeasurement.Run("CollectionPool.AlternatingWarmBuckets", cycle,
                operationsPerIteration: capacities.Length, expectZero: true);
        }

        [Test, Performance]
        public void SpawnDespawn_FirstDistinctBuckets([Values] CollectionKind kind, [Values(1, 16, 64)] int bucketCount)
        {
            CollectionPool pool = null;
            var adapter = new CollectionAdapter<object>();
            AllocationMeasurement.Run("CollectionPool.FirstDistinctBuckets", () =>
            {
                for (var capacity = 0; capacity < bucketCount; capacity++)
                    adapter.Despawn(pool, kind, adapter.Spawn(pool, kind, capacity));
            }, setUp: () => pool = new CollectionPool(), cleanUp: () => pool.Dispose(),
                iterations: 1, operationsPerIteration: bucketCount);
        }

        [Test, Performance]
        public void List_AfterHighWater_ReusesLargeBufferForSmallRequests()
        {
            const int peakCount = 4096;
            const int smallCount = 16;
            using var pool = new CollectionPool();
            var values = new object[peakCount];
            for (var i = 0; i < values.Length; i++)
                values[i] = new object();
            var peakList = pool.SpawnList<object>();
            for (var i = 0; i < peakCount; i++)
                peakList.Add(values[i]);
            var capacityAfterPeak = peakList.Capacity;
            pool.DespawnList(peakList);

            AllocationMeasurement.Run("CollectionPool.SmallCycleAfterHighWater", () =>
            {
                var list = pool.SpawnList<object>();
                for (var i = 0; i < smallCount; i++)
                    list.Add(values[i]);
                pool.DespawnList(list);
            }, expectZero: true);

            var reused = pool.SpawnList<object>();
            Assert.That(reused, Is.SameAs(peakList));
            Assert.That(reused.Capacity, Is.EqualTo(capacityAfterPeak));
            // Capacity is an observed element count, not an estimate of managed heap bytes.
            Measure.Custom(new SampleGroup("CollectionPool.RetainedListCapacity.Elements", SampleUnit.Undefined),
                reused.Capacity);
            pool.DespawnList(reused);
        }

        [Test, Performance]
        public void ClearOrDispose_WithActiveAndReturnedCollections(
            [Values(false, true)] bool dispose, [Values(1, 16, 64)] int count)
        {
            CollectionPool pool = null;
            AllocationMeasurement.Run(dispose ? "CollectionPool.Dispose" : "CollectionPool.Clear", () =>
            {
                if (dispose)
                    pool.Dispose();
                else
                    pool.Clear();
            },
                setUp: () =>
                {
                    pool = new CollectionPool();
                    for (var i = 0; i < count; i++)
                    {
                        pool.SpawnList<object>(i);
                        pool.DespawnList(pool.SpawnList<object>(i));
                    }
                }, cleanUp: () => pool.Dispose(), iterations: 1, expectZero: true);
        }

        private static void WithElementType(bool referenceElements, int count, Action<Array, ICollectionAdapter> action)
        {
            if (referenceElements)
            {
                var values = new object[count];
                for (var i = 0; i < count; i++)
                    values[i] = new object();
                action(values, new CollectionAdapter<object>());
            }
            else
            {
                var values = new int[count];
                for (var i = 0; i < count; i++)
                    values[i] = i;
                action(values, new CollectionAdapter<int>());
            }
        }

        private interface ICollectionAdapter
        {
            object Spawn(CollectionPool pool, CollectionKind kind, int capacity);
            void Fill(CollectionKind kind, object collection, Array values);
            void Despawn(CollectionPool pool, CollectionKind kind, object collection);
        }

        private sealed class CollectionAdapter<T> : ICollectionAdapter
        {
            public object Spawn(CollectionPool pool, CollectionKind kind, int capacity)
            {
                return kind switch
                {
                    CollectionKind.List => pool.SpawnList<T>(capacity),
                    CollectionKind.Array => pool.SpawnArray<T>(capacity),
                    CollectionKind.HashSet => pool.SpawnHashSet<T>(capacity),
                    CollectionKind.Dictionary => pool.SpawnDictionary<T, T>(capacity),
                    CollectionKind.Queue => pool.SpawnQueue<T>(capacity),
                    CollectionKind.Stack => pool.SpawnStack<T>(capacity),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind))
                };
            }

            public void Fill(CollectionKind kind, object collection, Array values)
            {
                var typedValues = (T[])values;
                switch (kind)
                {
                    case CollectionKind.List:
                        var list = (List<T>)collection;
                        for (var i = 0; i < typedValues.Length; i++)
                            list.Add(typedValues[i]);
                        break;
                    case CollectionKind.Array:
                        Array.Copy(typedValues, (T[])collection, typedValues.Length);
                        break;
                    case CollectionKind.HashSet:
                        var set = (HashSet<T>)collection;
                        for (var i = 0; i < typedValues.Length; i++)
                            set.Add(typedValues[i]);
                        break;
                    case CollectionKind.Dictionary:
                        var dictionary = (Dictionary<T, T>)collection;
                        for (var i = 0; i < typedValues.Length; i++)
                            dictionary.Add(typedValues[i], typedValues[i]);
                        break;
                    case CollectionKind.Queue:
                        var queue = (Queue<T>)collection;
                        for (var i = 0; i < typedValues.Length; i++)
                            queue.Enqueue(typedValues[i]);
                        break;
                    case CollectionKind.Stack:
                        var stack = (Stack<T>)collection;
                        for (var i = 0; i < typedValues.Length; i++)
                            stack.Push(typedValues[i]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind));
                }
            }

            public void Despawn(CollectionPool pool, CollectionKind kind, object collection)
            {
                switch (kind)
                {
                    case CollectionKind.List: pool.DespawnList((List<T>)collection); break;
                    case CollectionKind.Array: pool.DespawnArray((T[])collection); break;
                    case CollectionKind.HashSet: pool.DespawnHashSet((HashSet<T>)collection); break;
                    case CollectionKind.Dictionary: pool.DespawnDictionary((Dictionary<T, T>)collection); break;
                    case CollectionKind.Queue: pool.DespawnQueue((Queue<T>)collection); break;
                    case CollectionKind.Stack: pool.DespawnStack((Stack<T>)collection); break;
                    default: throw new ArgumentOutOfRangeException(nameof(kind));
                }
            }
        }
    }
}
