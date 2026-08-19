using System;
using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Components;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerInstantiationCpuTests
    {
        [Test, Performance]
        public void InstantiateGeneric_UnregisteredClass_AfterWarmup()
        {
            using var container = new Container();
            container.Instantiate<Service>();

            Measure.Method(() => container.Instantiate<Service>())
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void InstantiateGenericWithType_UnregisteredClass_AfterWarmup()
        {
            using var container = new Container();
            var serviceType = typeof(Service);
            container.Instantiate<Service>(serviceType);

            Measure.Method(() => container.Instantiate<Service>(serviceType))
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void InstantiateType_UnregisteredClass_AfterWarmup()
        {
            using var container = new Container();
            var serviceType = typeof(Service);
            container.Instantiate(serviceType);

            Measure.Method(() => container.Instantiate(serviceType))
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void InstantiateGeneric_WithPreResolvedCachedDependencies(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            Action instantiate = dependencyCount switch
            {
                1 => () => container.Instantiate<ServiceWith1Dependency>(),
                3 => () => container.Instantiate<ServiceWith3Dependencies>(),
                _ => () => container.Instantiate<ServiceWith10Dependencies>()
            };
            instantiate();

            Measure.Method(instantiate)
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void InstantiateType_WithPreResolvedCachedDependencies(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var serviceType = dependencyCount switch
            {
                1 => typeof(ServiceWith1Dependency),
                3 => typeof(ServiceWith3Dependencies),
                _ => typeof(ServiceWith10Dependencies)
            };
            container.Instantiate(serviceType);

            Measure.Method(() => container.Instantiate(serviceType))
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Performance]
        public void InstantiateGeneric_TransientChainIncludingRoot(int objectCount)
        {
            using var container = new Container();
            BindTransientChain(container, objectCount);
            Action instantiate = objectCount switch
            {
                1 => () => container.Instantiate<Service>(),
                5 => () => container.Instantiate<Chain5>(),
                _ => () => container.Instantiate<Chain10>()
            };
            instantiate();

            Measure.Method(instantiate)
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Performance]
        public void InstantiateType_TransientChainIncludingRoot(int objectCount)
        {
            using var container = new Container();
            var rootType = BindTransientChain(container, objectCount);
            container.Instantiate(rootType);

            Measure.Method(() => container.Instantiate(rootType))
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void InstantiateGameObject_WithInjectTargets(int targetCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, 1);
            var prefab = CreatePrefab(targetCount);
            GameObject instance = null;

            Measure.Method(() => instance = container.Instantiate(prefab))
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void InstantiateGenericComponent_WithInjectTargets(int targetCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, 1);
            var prefab = CreatePrefab(targetCount);
            var prefabComponent = prefab.GetComponent<PlainComponent>();
            PlainComponent instance = null;

            Measure.Method(() => instance = container.Instantiate<PlainComponent>(prefabComponent))
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void InstantiateComponent_WithInjectTargets(int targetCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, 1);
            var prefab = CreatePrefab(targetCount);
            Component prefabComponent = prefab.GetComponent<PlainComponent>();
            Component instance = null;

            Measure.Method(() => instance = container.Instantiate(prefabComponent))
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Instantiate", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void AddComponentGeneric_OnGameObject_WithCachedDependencies(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var gameObject = new GameObject("Target");
            Component instance = null;
            Action addComponent = dependencyCount switch
            {
                0 => () => instance = container.AddComponent<PlainComponent>(gameObject),
                1 => () => instance = container.AddComponent<ComponentWith1Dependency>(gameObject),
                3 => () => instance = container.AddComponent<ComponentWith3Dependencies>(gameObject),
                _ => () => instance = container.AddComponent<ComponentWith10Dependencies>(gameObject)
            };

            Measure.Method(addComponent)
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("AddComponent", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void AddComponentType_OnGameObject_WithCachedDependencies(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var gameObject = new GameObject("Target");
            var componentType = dependencyCount switch
            {
                0 => typeof(PlainComponent),
                1 => typeof(ComponentWith1Dependency),
                3 => typeof(ComponentWith3Dependencies),
                _ => typeof(ComponentWith10Dependencies)
            };
            Component instance = null;

            Measure.Method(() => instance = container.AddComponent(gameObject, componentType))
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("AddComponent", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [Performance]
        public void AddComponentGeneric_OnComponent_WithCachedDependency(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var gameObject = new GameObject("Target");
            Component host = gameObject.transform;
            Component instance = null;
            Action addComponent = dependencyCount == 0
                ? () => instance = container.AddComponent<PlainComponent>(host)
                : () => instance = container.AddComponent<ComponentWith1Dependency>(host);

            Measure.Method(addComponent)
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("AddComponent", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [Performance]
        public void AddComponentType_OnComponent_WithCachedDependency(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var gameObject = new GameObject("Target");
            Component host = gameObject.transform;
            var componentType = dependencyCount == 0
                ? typeof(PlainComponent)
                : typeof(ComponentWith1Dependency);
            Component instance = null;

            Measure.Method(() => instance = container.AddComponent(host, componentType))
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("AddComponent", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        private static GameObject CreatePrefab(int targetCount)
        {
            var prefab = new GameObject("Prefab");
            prefab.AddComponent<PlainComponent>();

            if (targetCount == 0)
                return prefab;

            var targets = new MonoBehaviour[targetCount];
            for (var i = 0; i < targetCount; i++)
                targets[i] = prefab.AddComponent<ComponentWith1Dependency>();

            var injectTargets = prefab.AddComponent<InjectTargets>();
            typeof(InjectTargets)
                .GetField("<Targets>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(injectTargets, targets);

            return prefab;
        }

        public sealed class PlainComponent : MonoBehaviour { }

        public sealed class ComponentWith1Dependency : MonoBehaviour
        {
            [Inject]
            private void Construct(Dependency1 dependency1) { }
        }

        public sealed class ComponentWith3Dependencies : MonoBehaviour
        {
            [Inject]
            private void Construct(Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3) { }
        }

        public sealed class ComponentWith10Dependencies : MonoBehaviour
        {
            [Inject]
            private void Construct(
                Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3,
                Dependency4 dependency4, Dependency5 dependency5, Dependency6 dependency6,
                Dependency7 dependency7, Dependency8 dependency8, Dependency9 dependency9,
                Dependency10 dependency10) { }
        }

        private static void BindCachedDependencies(Container container, int count)
        {
            var types = new[]
            {
                typeof(Dependency1), typeof(Dependency2), typeof(Dependency3), typeof(Dependency4), typeof(Dependency5),
                typeof(Dependency6), typeof(Dependency7), typeof(Dependency8), typeof(Dependency9), typeof(Dependency10)
            };

            for (var i = 0; i < count; i++)
            {
                container.Bind(types[i]).AsCached();
                container.Resolve(types[i]);
            }
        }

        private static Type BindTransientChain(Container container, int objectCount)
        {
            var types = new[]
            {
                typeof(Service), typeof(Chain2), typeof(Chain3), typeof(Chain4), typeof(Chain5),
                typeof(Chain6), typeof(Chain7), typeof(Chain8), typeof(Chain9), typeof(Chain10)
            };

            for (var i = 0; i < objectCount - 1; i++)
                container.Bind(types[i]).AsTransient();

            return types[objectCount - 1];
        }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }

        private sealed class Dependency1
        {
            [Preserve]
            public Dependency1() { }
        }

        private sealed class Dependency2
        {
            [Preserve]
            public Dependency2() { }
        }

        private sealed class Dependency3
        {
            [Preserve]
            public Dependency3() { }
        }

        private sealed class Dependency4
        {
            [Preserve]
            public Dependency4() { }
        }

        private sealed class Dependency5
        {
            [Preserve]
            public Dependency5() { }
        }

        private sealed class Dependency6
        {
            [Preserve]
            public Dependency6() { }
        }

        private sealed class Dependency7
        {
            [Preserve]
            public Dependency7() { }
        }

        private sealed class Dependency8
        {
            [Preserve]
            public Dependency8() { }
        }

        private sealed class Dependency9
        {
            [Preserve]
            public Dependency9() { }
        }

        private sealed class Dependency10
        {
            [Preserve]
            public Dependency10() { }
        }

        private sealed class ServiceWith1Dependency
        {
            [Preserve]
            public ServiceWith1Dependency(Dependency1 dependency) { }
        }

        private sealed class ServiceWith3Dependencies
        {
            [Preserve]
            public ServiceWith3Dependencies(Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3) { }
        }

        private sealed class ServiceWith10Dependencies
        {
            [Preserve]
            public ServiceWith10Dependencies(
                Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3,
                Dependency4 dependency4, Dependency5 dependency5, Dependency6 dependency6,
                Dependency7 dependency7, Dependency8 dependency8, Dependency9 dependency9,
                Dependency10 dependency10) { }
        }

        private sealed class Chain2
        {
            [Preserve]
            public Chain2(Service dependency) { }
        }

        private sealed class Chain3
        {
            [Preserve]
            public Chain3(Chain2 dependency) { }
        }

        private sealed class Chain4
        {
            [Preserve]
            public Chain4(Chain3 dependency) { }
        }

        private sealed class Chain5
        {
            [Preserve]
            public Chain5(Chain4 dependency) { }
        }

        private sealed class Chain6
        {
            [Preserve]
            public Chain6(Chain5 dependency) { }
        }

        private sealed class Chain7
        {
            [Preserve]
            public Chain7(Chain6 dependency) { }
        }

        private sealed class Chain8
        {
            [Preserve]
            public Chain8(Chain7 dependency) { }
        }

        private sealed class Chain9
        {
            [Preserve]
            public Chain9(Chain8 dependency) { }
        }

        private sealed class Chain10
        {
            [Preserve]
            public Chain10(Chain9 dependency) { }
        }
    }
}
