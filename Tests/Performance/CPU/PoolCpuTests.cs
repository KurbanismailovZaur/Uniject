using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class PoolCpuTests
    {
        [Test, Performance]
        public void Spawn_ReadyClrObject()
        {
            using var container = new Container();
            using var pool = CreateClrPool(container, 1);
            Service instance = null;

            Measure.Method(() => instance = pool.Spawn())
                .CleanUp(() => pool.Despawn(instance))
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase("Constructor")]
        [TestCase("Method")]
        [TestCase("Factory")]
        [Performance]
        public void Spawn_FirstWithZeroInitialSize(string source)
        {
            Container container = null;
            Pool<Service> pool = null;

            Measure.Method(() => pool.Spawn())
                .SetUp(() =>
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
                        case "Factory":
                            binding.FromFactory<ServiceCustomFactory>().AsCached();
                            break;
                    }

                    pool = container.Resolve<Pool<Service>>();
                })
                .CleanUp(() =>
                {
                    pool.Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false, -1)]
        [TestCase(1, true, -1)]
        [TestCase(16, true, -1)]
        [TestCase(256, true, -1)]
        [TestCase(16, true, 24)]
        [Performance]
        public void Spawn_ExhaustedPool_Expansion(int initialSize, bool expandByDoubling, int maxSize)
        {
            Container container = null;
            Pool<Service> pool = null;

            Measure.Method(() => pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    var binding = container.BindPool<Service, Pool<Service>>()
                        .WithInitialSize(initialSize).WithMaxSize(maxSize);
                    var from = expandByDoubling ? binding.ExpandByDoubling() : binding.ExpandByOne();
                    from.FromConstructor().AsCached();
                    pool = container.Resolve<Pool<Service>>();

                    for (var i = 0; i < initialSize; i++)
                        pool.Spawn();
                })
                .CleanUp(() =>
                {
                    pool.Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Spawn_ExhaustedPool_FromResolveTransient()
        {
            Container container = null;
            Pool<Service> pool = null;

            Measure.Method(() => pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().AsTransient();
                    container.BindPool<Service, Pool<Service>>()
                        .WithInitialSize(1).ExpandByOne().FromResolve().AsCached();
                    pool = container.Resolve<Pool<Service>>();
                    pool.Spawn();
                })
                .CleanUp(() =>
                {
                    pool.Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Despawn_ClrObject(int objectCount)
        {
            using var container = new Container();
            using var pool = CreateClrPool(container, objectCount);
            Service instance = null;

            for (var i = 1; i < objectCount; i++)
                pool.Spawn();

            Measure.Method(() => pool.Despawn(instance))
                .SetUp(() => instance = pool.Spawn())
                .SampleGroup(new SampleGroup("Despawn", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Adopt_ExternalClrObject()
        {
            Container container = null;
            Pool<Service> pool = null;
            Service instance = null;

            Measure.Method(() => pool.Adopt(instance))
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, 1);
                    instance = new Service();
                })
                .CleanUp(() =>
                {
                    pool.Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("Adopt", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Spawn_Empty_FromNewComponentOn()
        {
            var gameObject = new GameObject("Target");
            Container container = null;
            Pool<PoolComponent> pool = null;
            PoolComponent instance = null;

            Measure.Method(() => instance = pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindPool<PoolComponent, Pool<PoolComponent>>()
                        .WithInitialSize(0).ExpandByOne().FromNewComponentOn(gameObject).AsCached();
                    pool = container.Resolve<Pool<PoolComponent>>();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(instance);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [Test, Performance]
        public void Spawn_Empty_FromNewComponentOnNewGameObject()
        {
            Container container = null;
            Pool<PoolComponent> pool = null;
            PoolComponent instance = null;

            Measure.Method(() => instance = pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindPool<PoolComponent, Pool<PoolComponent>>()
                        .WithInitialSize(0).ExpandByOne().FromNewComponentOnNewGameObject().AsCached();
                    pool = container.Resolve<Pool<PoolComponent>>();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(instance.gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Spawn_Empty_FromComponentInNewPrefab()
        {
            var prefab = new GameObject("Prefab");
            prefab.AddComponent<PoolComponent>();
            Container container = null;
            Pool<PoolComponent> pool = null;
            PoolComponent instance = null;

            Measure.Method(() => instance = pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindPool<PoolComponent, Pool<PoolComponent>>()
                        .WithInitialSize(0).ExpandByOne().FromComponentInNewPrefab(prefab).AsCached();
                    pool = container.Resolve<Pool<PoolComponent>>();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(instance.gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [Test, Performance]
        public void Spawn_Empty_FromNewComponentOnNewPrefab()
        {
            var prefab = new GameObject("Prefab");
            Container container = null;
            Pool<PoolComponent> pool = null;
            PoolComponent instance = null;

            Measure.Method(() => instance = pool.Spawn())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindPool<PoolComponent, Pool<PoolComponent>>()
                        .WithInitialSize(0).ExpandByOne().FromNewComponentOnNewPrefab(prefab).AsCached();
                    pool = container.Resolve<Pool<PoolComponent>>();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(instance.gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Spawn_ReadyGameObject(bool withoutGameObjectActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("Pooled");
            var pool = CreateUnityPool(container, gameObject, withoutGameObjectActivation);
            pool.Adopt(gameObject);

            Measure.Method(() => pool.Spawn())
                .SetUp(() => gameObject.SetActive(false))
                .CleanUp(() => pool.Despawn(gameObject))
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Spawn_ReadyComponent(bool withoutGameObjectActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("Pooled");
            var instance = gameObject.AddComponent<PoolComponent>();
            var pool = CreateUnityPool(container, instance, withoutGameObjectActivation);
            pool.Adopt(instance);

            Measure.Method(() => pool.Spawn())
                .SetUp(() => gameObject.SetActive(false))
                .CleanUp(() => pool.Despawn(instance))
                .SampleGroup(new SampleGroup("Spawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Despawn_GameObject(bool withoutGameObjectActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("Pooled");
            var pool = CreateUnityPool(container, gameObject, withoutGameObjectActivation);
            pool.Adopt(gameObject);

            Measure.Method(() => pool.Despawn(gameObject))
                .SetUp(() =>
                {
                    pool.Spawn();
                    gameObject.SetActive(true);
                })
                .SampleGroup(new SampleGroup("Despawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Despawn_Component(bool withoutGameObjectActivation)
        {
            using var container = new Container();
            var gameObject = new GameObject("Pooled");
            var instance = gameObject.AddComponent<PoolComponent>();
            var pool = CreateUnityPool(container, instance, withoutGameObjectActivation);
            pool.Adopt(instance);

            Measure.Method(() => pool.Despawn(instance))
                .SetUp(() =>
                {
                    pool.Spawn();
                    gameObject.SetActive(true);
                })
                .SampleGroup(new SampleGroup("Despawn", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Adopt_GameObject(bool withoutGameObjectActivation)
        {
            var gameObject = new GameObject("Adopted");
            Container container = null;
            Pool<GameObject> pool = null;

            Measure.Method(() => pool.Adopt(gameObject))
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateUnityPool(container, gameObject, withoutGameObjectActivation);
                    gameObject.SetActive(true);
                })
                .CleanUp(() => container.Dispose())
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Adopt", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Adopt_Component(bool withoutGameObjectActivation)
        {
            var gameObject = new GameObject("Adopted");
            var instance = gameObject.AddComponent<PoolComponent>();
            Container container = null;
            Pool<PoolComponent> pool = null;

            Measure.Method(() => pool.Adopt(instance))
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateUnityPool(container, instance, withoutGameObjectActivation);
                    gameObject.SetActive(true);
                })
                .CleanUp(() => container.Dispose())
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Adopt", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Clear_Clr_AllSpawned(int count)
        {
            Container container = null;
            Pool<Service> pool = null;

            Measure.Method(() => pool.Clear())
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                    for (var i = 0; i < count; i++)
                        pool.Spawn();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Clear", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Clear_Clr_AllDespawned(int count)
        {
            Container container = null;
            Pool<Service> pool = null;
            var instances = new Service[count];

            Measure.Method(() => pool.Clear())
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                    for (var i = 0; i < count; i++)
                        instances[i] = pool.Spawn();
                    foreach (var instance in instances)
                        pool.Despawn(instance);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Clear", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Clear_Clr_Mixed(int count)
        {
            Container container = null;
            Pool<Service> pool = null;
            var instances = new Service[count];

            Measure.Method(() => pool.Clear())
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                    for (var i = 0; i < count; i++)
                        instances[i] = pool.Spawn();
                    for (var i = count / 2; i < count; i++)
                        pool.Despawn(instances[i]);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Clear", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Dispose_Clr_Filled(int count)
        {
            Container container = null;
            Pool<Service> pool = null;

            Measure.Method(() => pool.Dispose())
                .SetUp(() =>
                {
                    container = new Container();
                    pool = CreateClrPool(container, count);
                    for (var i = 0; i < count / 2; i++)
                        pool.Spawn();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        [UnityTest, Performance]
        public IEnumerator Clear_GameObjects()
        {
            var sampleGroup = new SampleGroup("Clear", SampleUnit.Nanosecond);

            for (var iteration = 0; iteration < 35; iteration++)
            {
                using var container = new Container();
                container.BindPool<GameObject, Pool<GameObject>>().WithInitialSize(100)
                    .FromMethod(_ => new GameObject("Pooled")).AsCached();
                var pool = container.Resolve<Pool<GameObject>>();
                for (var i = 0; i < 50; i++)
                    pool.Spawn();

                if (iteration < 5)
                    pool.Clear();
                else
                    using (Measure.Scope(sampleGroup))
                        pool.Clear();

                yield return null;
            }
        }

        [UnityTest, Performance]
        public IEnumerator Clear_Components()
        {
            var sampleGroup = new SampleGroup("Clear", SampleUnit.Nanosecond);

            for (var iteration = 0; iteration < 35; iteration++)
            {
                using var container = new Container();
                container.BindPool<PoolComponent, Pool<PoolComponent>>().WithInitialSize(100)
                    .FromNewComponentOnNewGameObject().AsCached();
                var pool = container.Resolve<Pool<PoolComponent>>();
                for (var i = 0; i < 50; i++)
                    pool.Spawn();

                if (iteration < 5)
                    pool.Clear();
                else
                    using (Measure.Scope(sampleGroup))
                        pool.Clear();

                yield return null;
            }
        }

        private static Pool<T> CreateUnityPool<T>(Container container, T instance, bool withoutGameObjectActivation)
            where T : class
        {
            var binding = container.BindPool<T, Pool<T>>().WithInitialSize(0).ExpandByOne();

            if (withoutGameObjectActivation)
                binding.WithoutGameObjectActivation().FromMethod(_ => instance).AsCached();
            else
                binding.FromMethod(_ => instance).AsCached();

            return container.Resolve<Pool<T>>();
        }

        private static Pool<Service> CreateClrPool(Container container, int initialSize)
        {
            container.BindPool<Service, Pool<Service>>()
                .WithInitialSize(initialSize).ExpandByOne().FromConstructor().AsCached();
            return container.Resolve<Pool<Service>>();
        }

        public sealed class PoolComponent : MonoBehaviour { }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }

        private sealed class ServiceCustomFactory : CustomFactory<Service>
        {
            public ServiceCustomFactory() { }

            public override Service Create() => new Service();
        }
    }
}
