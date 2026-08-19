using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Uniject.Tests.Performance.CPU
{
    public class CollectionPoolCpuTests
    {
        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnList(int capacity, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnList<int>(capacity))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnList(pool.SpawnList<int>(capacity));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnList", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnArray(int length, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnArray<int>(length))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnArray(pool.SpawnArray<int>(length));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnArray", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnHashSet(int capacity, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnHashSet<int>(capacity))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnHashSet(pool.SpawnHashSet<int>(capacity));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnHashSet", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnDictionary(int capacity, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnDictionary<int, int>(capacity))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnDictionary(pool.SpawnDictionary<int, int>(capacity));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnDictionary", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnQueue(int capacity, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnQueue<int>(capacity))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnQueue(pool.SpawnQueue<int>(capacity));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnQueue", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0, false)]
        [TestCase(16, false)]
        [TestCase(256, false)]
        [TestCase(4096, false)]
        [TestCase(0, true)]
        [TestCase(16, true)]
        [TestCase(256, true)]
        [TestCase(4096, true)]
        [Performance]
        public void SpawnStack(int capacity, bool returnedCollection)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.SpawnStack<int>(capacity))
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    if (returnedCollection)
                        pool.DespawnStack(pool.SpawnStack<int>(capacity));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("SpawnStack", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnList_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnList<int>(count);
            pool.DespawnList(collection);

            Measure.Method(() => pool.DespawnList(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnList<int>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(i);
                })
                .SampleGroup(new SampleGroup("DespawnList", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnList_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnList<object>(count);
            pool.DespawnList(collection);

            Measure.Method(() => pool.DespawnList(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnList<object>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(values[i]);
                })
                .SampleGroup(new SampleGroup("DespawnList", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnArray_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnArray<int>(count);
            pool.DespawnArray(collection);

            Measure.Method(() => pool.DespawnArray(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnArray<int>(count);
                    for (var i = 0; i < count; i++)
                        collection[i] = i;
                })
                .SampleGroup(new SampleGroup("DespawnArray", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnArray_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnArray<object>(count);
            pool.DespawnArray(collection);

            Measure.Method(() => pool.DespawnArray(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnArray<object>(count);
                    for (var i = 0; i < count; i++)
                        collection[i] = values[i];
                })
                .SampleGroup(new SampleGroup("DespawnArray", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnHashSet_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnHashSet<int>(count);
            pool.DespawnHashSet(collection);

            Measure.Method(() => pool.DespawnHashSet(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnHashSet<int>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(i);
                })
                .SampleGroup(new SampleGroup("DespawnHashSet", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnHashSet_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnHashSet<object>(count);
            pool.DespawnHashSet(collection);

            Measure.Method(() => pool.DespawnHashSet(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnHashSet<object>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(values[i]);
                })
                .SampleGroup(new SampleGroup("DespawnHashSet", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnDictionary_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnDictionary<int, int>(count);
            pool.DespawnDictionary(collection);

            Measure.Method(() => pool.DespawnDictionary(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnDictionary<int, int>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(i, i);
                })
                .SampleGroup(new SampleGroup("DespawnDictionary", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnDictionary_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnDictionary<int, object>(count);
            pool.DespawnDictionary(collection);

            Measure.Method(() => pool.DespawnDictionary(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnDictionary<int, object>(count);
                    for (var i = 0; i < count; i++)
                        collection.Add(i, values[i]);
                })
                .SampleGroup(new SampleGroup("DespawnDictionary", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnQueue_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnQueue<int>(count);
            pool.DespawnQueue(collection);

            Measure.Method(() => pool.DespawnQueue(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnQueue<int>(count);
                    for (var i = 0; i < count; i++)
                        collection.Enqueue(i);
                })
                .SampleGroup(new SampleGroup("DespawnQueue", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnQueue_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnQueue<object>(count);
            pool.DespawnQueue(collection);

            Measure.Method(() => pool.DespawnQueue(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnQueue<object>(count);
                    for (var i = 0; i < count; i++)
                        collection.Enqueue(values[i]);
                })
                .SampleGroup(new SampleGroup("DespawnQueue", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnStack_Int(int count)
        {
            using var pool = new CollectionPool();
            var collection = pool.SpawnStack<int>(count);
            pool.DespawnStack(collection);

            Measure.Method(() => pool.DespawnStack(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnStack<int>(count);
                    for (var i = 0; i < count; i++)
                        collection.Push(i);
                })
                .SampleGroup(new SampleGroup("DespawnStack", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(4096)]
        [Performance]
        public void DespawnStack_Object(int count)
        {
            using var pool = new CollectionPool();
            var values = CreateObjects(count);
            var collection = pool.SpawnStack<object>(count);
            pool.DespawnStack(collection);

            Measure.Method(() => pool.DespawnStack(collection))
                .SetUp(() =>
                {
                    collection = pool.SpawnStack<object>(count);
                    for (var i = 0; i < count; i++)
                        collection.Push(values[i]);
                })
                .SampleGroup(new SampleGroup("DespawnStack", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void Clear_WithReturnedBuckets(int bucketCount)
        {
            CollectionPool pool = null;

            Measure.Method(() => pool.Clear())
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    for (var i = 0; i < bucketCount; i++)
                        pool.DespawnList(pool.SpawnList<int>(i));
                })
                .CleanUp(() => pool.Dispose())
                .SampleGroup(new SampleGroup("Clear", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, 0)]
        [TestCase(10, 0)]
        [TestCase(100, 0)]
        [TestCase(0, 1)]
        [TestCase(0, 10)]
        [TestCase(0, 100)]
        [TestCase(1, 1)]
        [TestCase(10, 10)]
        [TestCase(100, 100)]
        [Performance]
        public void Dispose_WithSpawnedAndReturnedCollections(int spawnedCount, int returnedCount)
        {
            CollectionPool pool = null;
            var collections = new List<int>[spawnedCount + returnedCount];

            Measure.Method(() => pool.Dispose())
                .SetUp(() =>
                {
                    pool = new CollectionPool();
                    for (var i = 0; i < collections.Length; i++)
                        collections[i] = pool.SpawnList<int>(i);
                    for (var i = spawnedCount; i < collections.Length; i++)
                        pool.DespawnList(collections[i]);
                })
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        private static object[] CreateObjects(int count)
        {
            var values = new object[count];
            for (var i = 0; i < count; i++)
                values[i] = new object();

            return values;
        }
    }
}
