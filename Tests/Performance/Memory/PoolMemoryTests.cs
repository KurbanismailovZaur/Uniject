using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.Memory
{
    public class PoolMemoryTests
    {
        [Test, Performance]
        public void ResolvePool_InitialPreallocation([Values(0, 16, 256)] int initialSize)
        {
            Container container = null;
            Pool<Service> pool = null;
            AllocationMeasurement.Run("Pool.InitialPreallocation", () => pool = container.Resolve<Pool<Service>>(),
                setUp: () =>
                {
                    container = new Container();
                    container.BindPool<Service, Pool<Service>>()
                        .WithInitialSize(initialSize).FromConstructor().AsCached();
                }, cleanUp: () => DisposeClr(pool, container), iterations: 1);
        }

        [TestCase("Constructor")]
        [TestCase("Method")]
        [TestCase("Factory")]
        [Performance]
        public void Spawn_FirstWithZeroInitialSize(string source)
        {
            Container container = null;
            Pool<Service> pool = null;

            AllocationMeasurement.Run("Pool.FirstSpawn", () => pool.Spawn(),
                setUp: () =>
                {
                    container = new Container();
                    var binding = container.BindPool<Service, Pool<Service>>()
                        .WithInitialSize(0).ExpandByOne();
                    switch (source)
                    {
                        case "Constructor":
                            binding.FromConstructor().AsCached();
                            break;
                        case "Method":
                            binding.FromMethod(_ => new Service()).AsCached();
                            break;
                        default:
                            binding.FromFactory<ServiceFactory>().AsCached();
                            break;
                    }
                    pool = container.Resolve<Pool<Service>>();
                },
                cleanUp: () => DisposeClr(pool, container), iterations: 1);
        }

        [Test, Performance]
        public void Spawn_PreallocatedFirstBatch([Values(1, 16, 256)] int count)
        {
            Container container = null;
            Pool<Service> pool = null;
            var instances = new Service[count];

            // InitialSize creates objects, but does not pre-size the spawned-instance set.
            AllocationMeasurement.Run("Pool.PreallocatedFirstSpawn", () => SpawnBatch(pool, instances),
                setUp: () =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                },
                cleanUp: () =>
                {
                    Array.Clear(instances, 0, instances.Length);
                    DisposeClr(pool, container);
                },
                iterations: 1, operationsPerIteration: count);
        }

        [Test, Performance]
        public void SpawnDespawn_WarmBatch([Values(1, 16, 256)] int simultaneousCount)
        {
            using var container = new Container();
            using var pool = CreateClrPool(container, simultaneousCount);
            var instances = new Service[simultaneousCount];
            SpawnBatch(pool, instances);
            DespawnBatch(pool, instances);

            AllocationMeasurement.Run("Pool.WarmSpawnDespawn", () =>
            {
                SpawnBatch(pool, instances);
                DespawnBatch(pool, instances);
            }, operationsPerIteration: simultaneousCount, expectZero: true);
        }

        [TestCase(0, false, -1)]
        [TestCase(0, true, -1)]
        [TestCase(1, false, -1)]
        [TestCase(1, true, -1)]
        [TestCase(16, false, -1)]
        [TestCase(16, true, -1)]
        [TestCase(256, true, -1)]
        [TestCase(16, true, 24)]
        [Performance]
        public void Spawn_ExhaustedPool_Expansion(int initialSize, bool doubling, int maxSize)
        {
            Container container = null;
            Pool<Service> pool = null;
            var expectedCount = doubling ? Math.Max(1, initialSize * 2) : initialSize + 1;
            if (maxSize >= 0)
                expectedCount = Math.Min(expectedCount, maxSize);

            AllocationMeasurement.Run("Pool.Expansion", () => pool.Spawn(),
                setUp: () =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, initialSize, doubling, maxSize);
                    for (var i = 0; i < initialSize; i++)
                        pool.Spawn();
                },
                cleanUp: () =>
                {
                    try
                    {
                        Assert.That(pool.InstanceCount, Is.EqualTo(expectedCount));
                    }
                    finally
                    {
                        DisposeClr(pool, container);
                    }
                }, iterations: 1);
        }

        [Test, Performance]
        public void Spawn_ExhaustedPool_FromResolveTransient()
        {
            Container container = null;
            Pool<Service> pool = null;
            AllocationMeasurement.Run("Pool.Expansion.FromResolveTransient", () => pool.Spawn(),
                setUp: () =>
                {
                    container = new Container();
                    container.Bind<Service>().AsTransient();
                    container.BindPool<Service, Pool<Service>>()
                        .WithInitialSize(1).ExpandByOne().FromResolve().AsCached();
                    pool = container.Resolve<Pool<Service>>();
                    pool.Spawn();
                }, cleanUp: () => DisposeClr(pool, container), iterations: 1);
        }

        [Test, Performance]
        public void Adopt_AfterTrackingCapacityWarmup([Values(1, 16, 256)] int count)
        {
            using var container = new Container();
            using var pool = CreateClrPool(container, 0);
            var instances = new Service[count];
            for (var i = 0; i < count; i++)
            {
                instances[i] = new Service();
                pool.Adopt(instances[i]);
            }
            pool.Clear();

            AllocationMeasurement.Run("Pool.WarmAdopt", () =>
            {
                for (var i = 0; i < instances.Length; i++)
                    pool.Adopt(instances[i]);
            }, cleanUp: pool.Clear, operationsPerIteration: count, expectZero: true);
        }

        [Test, Performance]
        public void ClearOrDispose_ClrObjects(
            [Values(1, 16, 256)] int count,
            [Values("Spawned", "Despawned", "Mixed")] string state,
            [Values(false, true)] bool dispose)
        {
            Container container = null;
            Pool<Service> pool = null;
            AllocationMeasurement.Run(dispose ? "Pool.Dispose" : "Pool.Clear",
                () =>
                {
                    if (dispose)
                        pool.Dispose();
                    else
                        pool.Clear();
                },
                setUp: () =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                    var spawnedCount = state == "Spawned" ? count : state == "Mixed" ? count / 2 : 0;
                    for (var i = 0; i < spawnedCount; i++)
                        pool.Spawn();
                },
                cleanUp: () =>
                {
                    try
                    {
                        Assert.That(pool.InstanceCount, Is.Zero);
                    }
                    finally
                    {
                        DisposeClr(pool, container);
                    }
                }, iterations: 1, expectZero: true);
        }

        [Test, Performance]
        public void SpawnDespawn_WarmGameObject([Values(false, true)] bool withoutActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("PoolMemoryTest");
            try
            {
                var pool = CreateUnityPool(container, gameObject, withoutActivation);
                pool.Adopt(gameObject);
                pool.Despawn(pool.Spawn());
                AllocationMeasurement.Run("Pool.GameObject.WarmSpawnDespawn",
                    () => pool.Despawn(pool.Spawn()), expectZero: true);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test, Performance]
        public void SpawnDespawn_WarmComponent([Values(false, true)] bool withoutActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("PoolMemoryTest");
            try
            {
                var component = gameObject.AddComponent<PoolComponent>();
                var pool = CreateUnityPool(container, component, withoutActivation);
                pool.Adopt(component);
                pool.Despawn(pool.Spawn());
                AllocationMeasurement.Run("Pool.Component.WarmSpawnDespawn",
                    () => pool.Despawn(pool.Spawn()), expectZero: true);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [UnityTest, Performance]
        public IEnumerator Clear_GameObjects() => ClearUnityObjects(false, false);

        [UnityTest, Performance]
        public IEnumerator Dispose_GameObjects() => ClearUnityObjects(false, true);

        [UnityTest, Performance]
        public IEnumerator Clear_Components() => ClearUnityObjects(true, false);

        [UnityTest, Performance]
        public IEnumerator Dispose_Components() => ClearUnityObjects(true, true);

        private static IEnumerator ClearUnityObjects(bool components, bool dispose)
        {
            if (!Application.isPlaying)
                Assert.Ignore("Deferred pool destruction is measured in PlayMode.");

            const int count = 16;
            // Only the managed allocation around scheduling Destroy is measured here.
            // Native object destruction is checked after the deferred-destruction frame.
            for (var sample = 0; sample < 10; sample++)
            {
                var container = new Container();
                var objects = new GameObject[count];
                Pool pool = null;
                try
                {
                    if (components)
                    {
                        container.BindPool<PoolComponent, Pool<PoolComponent>>()
                            .WithInitialSize(0).FromNewComponentOnNewGameObject().AsCached();
                        var typedPool = container.Resolve<Pool<PoolComponent>>();
                        pool = typedPool;
                        for (var i = 0; i < count; i++)
                            objects[i] = typedPool.Spawn().gameObject;
                    }
                    else
                    {
                        container.BindPool<GameObject, Pool<GameObject>>()
                            .WithInitialSize(0).FromMethod(_ => new GameObject("PoolMemoryTest")).AsCached();
                        var typedPool = container.Resolve<Pool<GameObject>>();
                        pool = typedPool;
                        for (var i = 0; i < count; i++)
                            objects[i] = typedPool.Spawn();
                    }

                    Action action = dispose ? pool.Dispose : pool.Clear;
                    AllocationMeasurement.Run("Pool.Unity.DestroyScheduling", action,
                        iterations: 1, measurements: 1, warmup: 0);
                    yield return null;
                    foreach (var gameObject in objects)
                        Assert.That(gameObject == null, Is.True);
                }
                finally
                {
                    container.Dispose();
                    foreach (var gameObject in objects)
                    {
                        if (gameObject != null)
                            Object.DestroyImmediate(gameObject);
                    }
                }
            }
        }

        private static Pool<Service> CreateClrPool(Container container, int initialSize,
            bool doubling = false, int maxSize = -1)
        {
            var binding = container.BindPool<Service, Pool<Service>>()
                .WithInitialSize(initialSize).WithMaxSize(maxSize);
            var from = doubling ? binding.ExpandByDoubling() : binding.ExpandByOne();
            from.FromConstructor().AsCached();
            return container.Resolve<Pool<Service>>();
        }

        private static Pool<T> CreateUnityPool<T>(Container container, T instance, bool withoutActivation)
            where T : class
        {
            var binding = container.BindPool<T, Pool<T>>().WithInitialSize(0).ExpandByOne();
            if (withoutActivation)
                binding.WithoutGameObjectActivation().FromMethod(_ => instance).AsCached();
            else
                binding.FromMethod(_ => instance).AsCached();
            return container.Resolve<Pool<T>>();
        }

        private static void SpawnBatch(Pool<Service> pool, Service[] instances)
        {
            for (var i = 0; i < instances.Length; i++)
                instances[i] = pool.Spawn();
        }

        private static void DespawnBatch(Pool<Service> pool, Service[] instances)
        {
            for (var i = 0; i < instances.Length; i++)
                pool.Despawn(instances[i]);
        }

        private static void DisposeClr(Pool<Service> pool, Container container)
        {
            try
            {
                pool?.Dispose();
            }
            finally
            {
                container?.Dispose();
            }
        }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }

        private sealed class ServiceFactory : CustomFactory<Service>
        {
            [Preserve]
            public ServiceFactory() { }

            public override Service Create() => new Service();
        }

        public sealed class PoolComponent : MonoBehaviour { }
    }
}
