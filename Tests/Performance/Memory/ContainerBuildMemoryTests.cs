using NUnit.Framework;
using Uniject.Reflection;
using Unity.PerformanceTesting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerBuildMemoryTests
    {
        [Test, Performance]
        public void Build_WithLazyBindings([Values(0, 10, 100, 1000)] int bindingCount)
        {
            var types = CreateTypes(typeof(Contract<,,>), bindingCount);
            Container container = null;
            AllocationMeasurement.Run("Build.Lazy", () => container.Build(), setUp: () =>
            {
                container = new Container();
                foreach (var type in types) container.Bind(type).AsCached();
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Build_NonLazyWithWarmReflection([Values(1, 10, 100)] int objectCount, [Values] bool cached)
        {
            var types = CreateTypes(typeof(Contract<,,>), objectCount);
            foreach (var type in types) ReflectionCache.GetConstructorInjectionData(type);
            Container container = null;
            AllocationMeasurement.Run("Build.NonLazy", () => container.Build(), setUp: () =>
            {
                container = new Container();
                foreach (var type in types)
                {
                    var binding = container.Bind(type);
                    if (cached) binding.AsCached().NonLazy(); else binding.AsTransient().NonLazy();
                }
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Build_InjectionQueue([Values(1, 10, 100, 1000)] int objectCount)
        {
            var targets = new object[objectCount];
            for (var i = 0; i < targets.Length; i++) targets[i] = new InjectionTarget();
            ReflectionCache.GetMethodInjectionData(typeof(InjectionTarget));
            var dependency = new Dependency();
            Container container = null;
            AllocationMeasurement.Run("Build.InjectionQueue", () => container.Build(), setUp: () =>
            {
                container = new Container();
                container.Bind<Dependency>().FromInstance(dependency);
                container.Resolve<Dependency>();
                container.AddToInjectionQueue(targets);
            }, cleanUp: () => container?.Dispose(), iterations: 1);
            foreach (InjectionTarget target in targets) Assert.That(target.Dependency, Is.SameAs(dependency));
        }

        [Test, Performance]
        public void Build_EntryPointsWithWarmReflection([Values(1, 10, 100)] int entryPointCount)
        {
            var types = CreateTypes(typeof(EntryPoint<,,>), entryPointCount);
            foreach (var type in types) ReflectionCache.GetConstructorInjectionData(type);
            Container container = null;
            AllocationMeasurement.Run("Build.EntryPoints", () => container.Build(), setUp: () =>
            {
                container = new Container();
                foreach (var type in types) container.Bind(type).AsEntryPoint();
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Build_AlreadyBuiltContainer()
        {
            using var container = new Container();
            container.Bind<Dependency>().AsCached().NonLazy();
            var target = new InjectionTarget();
            container.AddToInjectionQueue(target);
            container.Build();
            AllocationMeasurement.Run("Build.Repeated", () => container.Build(), expectZero: true);
            Assert.That(target.Dependency, Is.Not.Null);
        }
    }
}
