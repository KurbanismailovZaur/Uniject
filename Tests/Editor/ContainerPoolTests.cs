using System;
using NUnit.Framework;
using Uniject.Bindings.Pools;
using Uniject.Contexts;
using Uniject.Tests.Fixtures;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests
{
    public class ContainerPoolTests : ContainerPoolTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            DuplicateProductFactory.Reset();
            PooledScriptFactory.DestroyCreatedInstances();
        }

        [TearDown]
        public void TearDown()
        {
            DuplicateProductFactory.Reset();
            PooledScriptFactory.DestroyCreatedInstances();
        }

        [Test]
        public void ResolvePool_AsCached_ReturnsSamePool()
        {
            var container = new Container();
            container.BindPool<Product, ProductPool>().FromConstructor().AsCached();

            var first = container.Resolve<ProductPool>();
            var second = container.Resolve<ProductPool>();

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ResolvePool_AsTransient_ReturnsDifferentPools()
        {
            var container = new Container();
            container.BindPool<Product, ProductPool>().FromConstructor().AsTransient();

            var first = container.Resolve<ProductPool>();
            var second = container.Resolve<ProductPool>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void ResolvePool_WithInitialSize_PrewarmsPool()
        {
            var pool = CreateProductPool(initialSize: 3);

            Assert.That(pool.InitialSize, Is.EqualTo(3));
            Assert.That(pool.InstanceCount, Is.EqualTo(3));
            Assert.That(pool.ResetCallsCount, Is.EqualTo(3));
        }

        [Test]
        public void ResolvePool_WhenInitialSizeExceedsMaxSize_CapsInstanceCountAtMaxSize()
        {
            var pool = CreateProductPool(initialSize: 3, maxSize: 2);

            Assert.That(pool.InitialSize, Is.EqualTo(3));
            Assert.That(pool.MaxSize, Is.EqualTo(2));
            Assert.That(pool.InstanceCount, Is.EqualTo(2));
        }

        [Test]
        public void Spawn_WhenInstanceIsAvailable_DoesNotExpandPool()
        {
            var pool = CreateProductPool(initialSize: 1);

            var instance = pool.Spawn();

            Assert.That(instance, Is.Not.Null);
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Spawn_ExpandByOne_AddsOneInstance()
        {
            var pool = CreateProductPool(initialSize: 1, expandType: ExpandType.ByOne);
            var first = pool.Spawn();

            var second = pool.Spawn();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(pool.InstanceCount, Is.EqualTo(2));
        }

        [Test]
        public void Spawn_ExpandByDoubling_DoublesInstanceCount()
        {
            var pool = CreateProductPool(initialSize: 2, expandType: ExpandType.ByDoubling);
            var first = pool.Spawn();
            var second = pool.Spawn();

            var third = pool.Spawn();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(third, Is.Not.SameAs(first));
            Assert.That(third, Is.Not.SameAs(second));
            Assert.That(pool.InstanceCount, Is.EqualTo(4));
        }

        [Test]
        public void Spawn_WhenMaximumSizeIsReached_ThrowsInvalidOperationException()
        {
            var pool = CreateProductPool(initialSize: 2, maxSize: 2);
            pool.Spawn();
            pool.Spawn();

            Assert.That(
                () => pool.Spawn(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Despawn_ReturnsInstanceAndResetsItsState()
        {
            var pool = CreateProductPool(initialSize: 1);
            var instance = pool.Spawn();
            instance.Value = 42;

            pool.Despawn(instance);
            var reusedInstance = pool.Spawn();

            Assert.That(reusedInstance, Is.SameAs(instance));
            Assert.That(reusedInstance.Value, Is.Zero);
            Assert.That(pool.ResetCallsCount, Is.EqualTo(2));
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_WithNull_ThrowsArgumentNullException()
        {
            var pool = CreateProductPool();

            Assert.That(
                () => pool.Despawn(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Despawn_WithForeignInstance_ThrowsInvalidOperationException()
        {
            var pool = CreateProductPool();

            Assert.That(
                () => pool.Despawn(new Product()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Despawn_WhenInstanceWasAlreadyDespawned_ThrowsInvalidOperationException()
        {
            var pool = CreateProductPool(initialSize: 1);
            var instance = pool.Spawn();
            pool.Despawn(instance);

            Assert.That(
                () => pool.Despawn(instance),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Adopt_AddsInstanceAsDespawnedAndResetsItsState()
        {
            var pool = CreateProductPool();
            var instance = new Product { Value = 42 };

            pool.Adopt(instance);

            Assert.That(instance.Value, Is.Zero);
            Assert.That(pool.ResetCallsCount, Is.EqualTo(1));
            Assert.That(pool.InstanceCount, Is.EqualTo(1));

            var spawnedInstance = pool.Spawn();
            Assert.That(spawnedInstance, Is.SameAs(instance));
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Adopt_WhenInstanceIsAlreadyDespawned_ThrowsInvalidOperationException()
        {
            var pool = CreateProductPool();
            var instance = new Product();
            pool.Adopt(instance);

            Assert.That(
                () => pool.Adopt(instance),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Adopt_WhenInstanceIsSpawned_ThrowsInvalidOperationException()
        {
            var pool = CreateProductPool();
            var instance = new Product();
            pool.Adopt(instance);
            pool.Spawn();

            Assert.That(
                () => pool.Adopt(instance),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Adopt_WhenMaximumSizeIsReached_ThrowsWithoutResettingInstance()
        {
            var pool = CreateProductPool(initialSize: 1, maxSize: 1);
            var instance = new Product { Value = 42 };

            Assert.That(
                () => pool.Adopt(instance),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(instance.Value, Is.EqualTo(42));
            Assert.That(pool.ResetCallsCount, Is.EqualTo(1));
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Adopt_WithNull_ThrowsArgumentNullException()
        {
            var pool = CreateProductPool();

            Assert.That(
                () => pool.Adopt(null),
                Throws.TypeOf<ArgumentNullException>());
            Assert.That(pool.InstanceCount, Is.Zero);
        }

        [Test]
        public void Adopt_WhenDistinctInstancesAreEqual_TracksThemByReference()
        {
            var container = new Container();
            container.BindPool<EquatableProduct, EquatableProductPool>()
                .ExpandByOne()
                .FromConstructor()
                .AsCached();
            var pool = container.Resolve<EquatableProductPool>();
            var first = new EquatableProduct();
            var second = new EquatableProduct();

            pool.Adopt(first);
            pool.Adopt(second);

            Assert.That(pool.InstanceCount, Is.EqualTo(2));
            Assert.That(pool.Spawn(), Is.SameAs(second));
            Assert.That(pool.Spawn(), Is.SameAs(first));
        }

        [Test]
        public void Adopt_Component_DeactivatesItAndSpawnReactivatesIt()
        {
            var instance = new GameObject("Adopted").AddComponent<Script>();

            try
            {
                var container = new Container();
                container.BindPool<Script, PooledScriptPool>()
                    .ExpandByOne()
                    .FromFactory<PooledScriptFactory>()
                    .AsCached();
                var pool = container.Resolve<PooledScriptPool>();

                pool.Adopt(instance);

                Assert.That(instance.gameObject.activeSelf, Is.False);
                Assert.That(pool.InstanceCount, Is.EqualTo(1));

                var spawnedInstance = pool.Spawn();
                Assert.That(spawnedInstance, Is.SameAs(instance));
                Assert.That(instance.gameObject.activeSelf, Is.True);
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);
            }
        }

        [Test]
        public void Adopt_WithDestroyedComponent_ThrowsArgumentNullException()
        {
            var instance = new GameObject("Destroyed").AddComponent<Script>();
            var container = new Container();
            container.BindPool<Script, PooledScriptPool>()
                .ExpandByOne()
                .FromFactory<PooledScriptFactory>()
                .AsCached();
            var pool = container.Resolve<PooledScriptPool>();
            UnityEngine.Object.DestroyImmediate(instance.gameObject);

            Assert.That(
                () => pool.Adopt(instance),
                Throws.TypeOf<ArgumentNullException>());
            Assert.That(pool.InstanceCount, Is.Zero);
        }

        [Test]
        public void Spawn_WhenFactoryReturnsAlreadySpawnedInstance_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindPool<Product, ProductPool>()
                .WithInitialSize(1)
                .ExpandByOne()
                .FromFactory<DuplicateProductFactory>()
                .AsCached();
            var pool = container.Resolve<ProductPool>();
            pool.Spawn();

            Assert.That(
                () => pool.Spawn(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ResolvePool_WhenFactoryReturnsNull_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindPool<Product, ProductPool>()
                .WithInitialSize(1)
                .ExpandByOne()
                .FromFactory<NullProductFactory>()
                .AsCached();

            Assert.That(
                () => container.Resolve<ProductPool>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ResolvePool_WhenDistinctInstancesAreEqual_TracksThemByReference()
        {
            var container = new Container();
            container.BindPool<EquatableProduct, EquatableProductPool>()
                .WithInitialSize(2)
                .ExpandByOne()
                .FromConstructor()
                .AsCached();

            var pool = container.Resolve<EquatableProductPool>();
            var first = pool.Spawn();
            var second = pool.Spawn();

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(pool.InstanceCount, Is.EqualTo(2));
        }

        [Test]
        public void Clear_WhenPoolContainsSpawnedAndDespawnedInstances_RemovesAllInstances()
        {
            var pool = CreateProductPool(initialSize: 2);
            var spawned = pool.Spawn();
            var despawned = pool.Spawn();
            pool.Despawn(despawned);

            pool.Clear();

            Assert.That(pool.InstanceCount, Is.Zero);

            var newInstance = pool.Spawn();
            Assert.That(newInstance, Is.Not.SameAs(spawned));
            Assert.That(newInstance, Is.Not.SameAs(despawned));
        }

        [Test]
        public void Dispose_ClearsPool()
        {
            var pool = CreateProductPool(initialSize: 2);
            pool.Spawn();

            pool.Dispose();

            Assert.That(pool.InstanceCount, Is.Zero);
        }

        [Test]
        public void SpawnAndDespawn_Component_UpdatesGameObjectActiveState()
        {
            var container = new Container();
            container.BindPool<Script, PooledScriptPool>()
                .WithInitialSize(1)
                .ExpandByOne()
                .FromFactory<PooledScriptFactory>()
                .AsCached();
            var pool = container.Resolve<PooledScriptPool>();
            var createdInstance = PooledScriptFactory.CreatedInstances[0];

            Assert.That(createdInstance.gameObject.activeSelf, Is.False);

            var spawnedInstance = pool.Spawn();
            Assert.That(spawnedInstance, Is.SameAs(createdInstance));
            Assert.That(spawnedInstance.gameObject.activeSelf, Is.True);

            pool.Despawn(spawnedInstance);
            Assert.That(spawnedInstance.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Spawn_Component_IgnoresInheritedContainerParentAndContext()
        {
            var contextObject = new GameObject("GameObjectContext");
            var containerParent = new GameObject("ContainerParent").transform;
            var instance = default(Script);

            try
            {
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context, parentTransformForGameObjects: containerParent);
                context.Initialize();
                context.Install();
                var childContainer = new Container(context.Container);
                childContainer.BindPool<Script, PooledScriptPool>()
                    .ExpandByOne()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();

                var pool = childContainer.Resolve<PooledScriptPool>();
                instance = pool.Spawn();

                Assert.That(instance.transform.parent, Is.Null);
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);

                UnityEngine.Object.DestroyImmediate(containerParent.gameObject);
                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Spawn_Component_IgnoresInheritedSceneContext()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var instance = default(Script);

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);

                var context = contextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Install();
                var childContainer = new Container(context.Container);
                childContainer.BindPool<Script, PooledScriptPool>()
                    .ExpandByOne()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();

                var pool = childContainer.Resolve<PooledScriptPool>();
                instance = pool.Spawn();

                Assert.That(instance.gameObject.scene, Is.Not.EqualTo(contextScene));
                Assert.That(instance.transform.parent, Is.Null);
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);

                UnityEngine.Object.DestroyImmediate(contextObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void WithInitialSize_WhenValueIsNegative_ThrowsArgumentOutOfRangeException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindPool<Product, ProductPool>().WithInitialSize(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void WithMaxSize_WhenValueIsLessThanMinusOne_ThrowsArgumentOutOfRangeException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindPool<Product, ProductPool>().WithMaxSize(-2),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
