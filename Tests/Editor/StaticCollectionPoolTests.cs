using System;
using System.Reflection;
using NUnit.Framework;

namespace Uniject.Tests
{
    public class StaticCollectionPoolTests
    {
        private const string StaticCollectionsTypeName = "Uniject.StaticCollections";
        private const string CollectionPoolFieldName = "collectionPool";

        [Test]
        public void StaticCollections_WhenInitialized_ProvidesCollectionPool()
        {
            var first = GetCollectionPool();
            var second = GetCollectionPool();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void StaticCollections_AcrossAccesses_SharesCollectionPoolState()
        {
            var originalPool = GetCollectionPool();
            var isolatedPool = new CollectionPool();

            try
            {
                SetCollectionPool(isolatedPool);
                var firstAccess = GetCollectionPool();
                var list = firstAccess.SpawnList<int>(4);
                list.Add(1);
                firstAccess.DespawnList(list);

                var secondAccess = GetCollectionPool();
                var reused = secondAccess.SpawnList<int>(4);

                Assert.That(secondAccess, Is.SameAs(firstAccess));
                Assert.That(reused, Is.SameAs(list));
                Assert.That(reused, Is.Empty);
            }
            finally
            {
                SetCollectionPool(originalPool);
                isolatedPool.Dispose();
            }
        }

        private static CollectionPool GetCollectionPool()
        {
            return (CollectionPool)GetCollectionPoolField().GetValue(null);
        }

        private static void SetCollectionPool(CollectionPool collectionPool)
        {
            GetCollectionPoolField().SetValue(null, collectionPool);
        }

        private static FieldInfo GetCollectionPoolField()
        {
            var staticCollectionsType = typeof(CollectionPool).Assembly.GetType(
                StaticCollectionsTypeName,
                throwOnError: true);
            var field = staticCollectionsType.GetField(
                CollectionPoolFieldName,
                BindingFlags.Static | BindingFlags.NonPublic);

            if (field == null)
                throw new MissingFieldException(StaticCollectionsTypeName, CollectionPoolFieldName);

            return field;
        }
    }
}
