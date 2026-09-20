using System;
using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Components;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerInstantiationMemoryTests
    {
        [Test, Performance]
        public void Instantiate_WithCachedDependencies([Values(0, 1, 3, 10)] int dependencyCount)
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            var type = ServiceType(dependencyCount);
            var direct = DirectConstructor(dependencyCount, dependency);
            object result = null;
            AllocationMeasurement.Run("DirectNew", () => result = direct());
            AllocationMeasurement.Run("Instantiate", () => result = container.Instantiate(type));
            Assert.That(result, Is.TypeOf(type));
        }

        [Test, Performance]
        public void Instantiate_GenericOverloads([Values] bool explicitType)
        {
            using var container = new Container();
            object result = null;
            Action instantiate = explicitType
                ? () => result = container.Instantiate<IService>(typeof(Service))
                : () => result = container.Instantiate<Service>();
            AllocationMeasurement.Run("Instantiate.Generic", instantiate);
            Assert.That(result, Is.TypeOf<Service>());
        }

        [Test, Performance]
        public void Instantiate_TransientChainIncludingRoot([Values(1, 5, 10)] int objectCount)
        {
            using var container = new Container();
            var types = ChainTypes(objectCount);
            for (var i = 0; i < types.Length - 1; i++) container.Bind(types[i]).AsTransient();
            var root = types[types.Length - 1];
            object result = null;
            AllocationMeasurement.Run("Instantiate.TransientChain", () => result = container.Instantiate(root));
            Assert.That(result, Is.TypeOf(root));
        }

        [Test, Performance]
        public void Instantiate_PrefabWithInjectTargets([Values(0, 1, 10)] int targetCount, [Values] bool componentOverload)
        {
            using var container = new Container();
            container.Bind<Dependency>().FromInstance(new Dependency());
            var prefab = new GameObject("MemoryPrefab");
            GameObject instance = null;
            try
            {
                if (targetCount > 0)
                {
                    var targets = new MonoBehaviour[targetCount];
                    for (var i = 0; i < targets.Length; i++) targets[i] = prefab.AddComponent<InjectedComponent>();
                    var injectTargets = prefab.AddComponent<InjectTargets>();
                    typeof(InjectTargets).GetField("<Targets>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(injectTargets, targets);
                }
                Action instantiate = componentOverload
                    ? () => instance = container.Instantiate(prefab.transform).gameObject
                    : () => instance = container.Instantiate(prefab);
                AllocationMeasurement.Run("Instantiate.Prefab.Managed", instantiate,
                    cleanUp: () => { if (instance != null) Object.DestroyImmediate(instance); }, iterations: 1);
            }
            finally
            {
                if (instance != null) Object.DestroyImmediate(instance);
                Object.DestroyImmediate(prefab);
            }
        }

        [Test, Performance]
        public void AddComponent_WithOrWithoutInjection([Values] bool withInjection, [Values] bool generic)
        {
            using var container = new Container();
            container.Bind<Dependency>().FromInstance(new Dependency());
            var gameObject = new GameObject("MemoryComponentHost");
            Component instance = null;
            try
            {
                Action add;
                if (withInjection)
                    add = generic ? () => instance = container.AddComponent<InjectedComponent>(gameObject)
                        : () => instance = container.AddComponent(gameObject, typeof(InjectedComponent));
                else
                    add = generic ? () => instance = container.AddComponent<PlainComponent>(gameObject)
                        : () => instance = container.AddComponent(gameObject, typeof(PlainComponent));
                AllocationMeasurement.Run("AddComponent.Managed", add,
                    cleanUp: () => { if (instance != null) Object.DestroyImmediate(instance); }, iterations: 1);
            }
            finally { Object.DestroyImmediate(gameObject); }
        }

        public sealed class PlainComponent : MonoBehaviour { }

        public sealed class InjectedComponent : MonoBehaviour
        {
            [Inject, Preserve] private void Construct(Dependency dependency) { }
        }
    }
}
