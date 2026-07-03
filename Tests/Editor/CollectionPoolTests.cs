using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Uniject.Tests
{
    public class CollectionPoolTests
    {
        private CollectionPool _pool;

        [SetUp]
        public void SetUp()
        {
            _pool = new CollectionPool();
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }

        [Test]
        public void SpawnList_AfterDespawn_ReturnsSameClearedList()
        {
            var list = _pool.SpawnList<object>(8);
            list.Add(new object());

            _pool.DespawnList(list);
            var reused = _pool.SpawnList<object>(8);

            Assert.That(reused, Is.SameAs(list));
            Assert.That(reused, Is.Empty);
            Assert.That(reused.Capacity, Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void SpawnArray_AfterDespawn_ReturnsSameClearedArrayWithExactLength()
        {
            var array = _pool.SpawnArray<object>(3);
            array[0] = new object();
            array[1] = new object();
            array[2] = new object();

            _pool.DespawnArray(array);
            var reused = _pool.SpawnArray<object>(3);

            Assert.That(reused, Is.SameAs(array));
            Assert.That(reused.Length, Is.EqualTo(3));
            Assert.That(reused[0], Is.Null);
            Assert.That(reused[1], Is.Null);
            Assert.That(reused[2], Is.Null);
        }

        [Test]
        public void SpawnHashSet_AfterDespawn_ReturnsSameClearedHashSet()
        {
            var set = _pool.SpawnHashSet<int>(8);
            set.Add(1);

            _pool.DespawnHashSet(set);
            var reused = _pool.SpawnHashSet<int>(8);

            Assert.That(reused, Is.SameAs(set));
            Assert.That(reused, Is.Empty);
        }

        [Test]
        public void SpawnDictionary_AfterDespawn_ReturnsSameClearedDictionary()
        {
            var dictionary = _pool.SpawnDictionary<int, object>(8);
            dictionary.Add(1, new object());

            _pool.DespawnDictionary(dictionary);
            var reused = _pool.SpawnDictionary<int, object>(8);

            Assert.That(reused, Is.SameAs(dictionary));
            Assert.That(reused, Is.Empty);
        }

        [Test]
        public void SpawnQueue_AfterDespawn_ReturnsSameClearedQueue()
        {
            var queue = _pool.SpawnQueue<object>(8);
            queue.Enqueue(new object());

            _pool.DespawnQueue(queue);
            var reused = _pool.SpawnQueue<object>(8);

            Assert.That(reused, Is.SameAs(queue));
            Assert.That(reused, Is.Empty);
        }

        [Test]
        public void SpawnStack_AfterDespawn_ReturnsSameClearedStack()
        {
            var stack = _pool.SpawnStack<object>(8);
            stack.Push(new object());

            _pool.DespawnStack(stack);
            var reused = _pool.SpawnStack<object>(8);

            Assert.That(reused, Is.SameAs(stack));
            Assert.That(reused, Is.Empty);
        }

        [Test]
        public void SpawnList_WhenSeveralListsAreRequestedBeforeDespawn_ReturnsDifferentInstances()
        {
            var first = _pool.SpawnList<int>();
            var second = _pool.SpawnList<int>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void SpawnList_AfterSeveralDespawns_ReusesCollectionsInLifoOrder()
        {
            var first = _pool.SpawnList<int>();
            var second = _pool.SpawnList<int>();
            _pool.DespawnList(first);
            _pool.DespawnList(second);

            Assert.That(_pool.SpawnList<int>(), Is.SameAs(second));
            Assert.That(_pool.SpawnList<int>(), Is.SameAs(first));
        }

        [Test]
        public void SpawnList_WithDifferentCapacities_UsesDifferentBuckets()
        {
            var capacityFour = _pool.SpawnList<int>(4);
            _pool.DespawnList(capacityFour);

            var capacityEight = _pool.SpawnList<int>(8);

            Assert.That(capacityEight, Is.Not.SameAs(capacityFour));
            Assert.That(capacityEight.Capacity, Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void SpawnList_WithDifferentElementTypes_UsesDifferentBuckets()
        {
            var integers = _pool.SpawnList<int>();
            _pool.DespawnList(integers);

            var strings = _pool.SpawnList<string>();

            Assert.That(strings, Is.Not.SameAs(integers));
        }

        [Test]
        public void SpawnArray_WithDifferentLengths_UsesDifferentBuckets()
        {
            var lengthThree = _pool.SpawnArray<int>(3);
            _pool.DespawnArray(lengthThree);

            var lengthFour = _pool.SpawnArray<int>(4);
            var reusedLengthThree = _pool.SpawnArray<int>(3);

            Assert.That(lengthFour.Length, Is.EqualTo(4));
            Assert.That(lengthFour, Is.Not.SameAs(lengthThree));
            Assert.That(reusedLengthThree, Is.SameAs(lengthThree));
        }

        [Test]
        public void Spawn_WithNegativeCapacityOrLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => _pool.SpawnList<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _pool.SpawnArray<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _pool.SpawnHashSet<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _pool.SpawnDictionary<int, int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _pool.SpawnQueue<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _pool.SpawnStack<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Despawn_WithNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _pool.DespawnList<int>(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _pool.DespawnArray<int>(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _pool.DespawnHashSet<int>(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _pool.DespawnDictionary<int, int>(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _pool.DespawnQueue<int>(null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _pool.DespawnStack<int>(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Despawn_WithForeignCollection_ThrowsInvalidOperationException()
        {
            Assert.That(
                () => _pool.DespawnList(new List<int>()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Despawn_WhenCollectionWasAlreadyDespawned_ThrowsInvalidOperationException()
        {
            var list = _pool.SpawnList<int>();
            _pool.DespawnList(list);

            Assert.That(
                () => _pool.DespawnList(list),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void DespawnArray_WithCovariantElementType_ThrowsAndKeepsArrayTracked()
        {
            var strings = _pool.SpawnArray<string>(1);
            object[] objects = strings;

            Assert.That(
                () => _pool.DespawnArray(objects),
                Throws.TypeOf<InvalidOperationException>());

            Assert.That(() => _pool.DespawnArray(strings), Throws.Nothing);
        }

        [Test]
        public void Clear_DropsDespawnedCollectionsButKeepsSpawnedCollectionsTracked()
        {
            var cachedList = _pool.SpawnList<int>();
            var spawnedArray = _pool.SpawnArray<int>(2);
            _pool.DespawnList(cachedList);

            _pool.Clear();

            var newList = _pool.SpawnList<int>();
            Assert.That(newList, Is.Not.SameAs(cachedList));

            _pool.DespawnArray(spawnedArray);
            Assert.That(_pool.SpawnArray<int>(2), Is.SameAs(spawnedArray));
        }

        [Test]
        public void Dispose_WhenCalledTwice_DoesNotThrow()
        {
            _pool.Dispose();

            Assert.That(() => _pool.Dispose(), Throws.Nothing);
        }

        [Test]
        public void Operations_AfterDispose_ThrowObjectDisposedException()
        {
            var list = _pool.SpawnList<int>();
            _pool.Dispose();

            Assert.That(() => _pool.SpawnList<int>(), Throws.TypeOf<ObjectDisposedException>());
            Assert.That(() => _pool.DespawnList(list), Throws.TypeOf<ObjectDisposedException>());
            Assert.That(() => _pool.Clear(), Throws.TypeOf<ObjectDisposedException>());
        }
    }
}
