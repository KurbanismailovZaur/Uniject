using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerInjectionMemoryTests
    {
        [Test, Performance]
        public void Inject_WithCachedDependencies_AfterWarmup([Values(0, 1, 3, 10)] int dependencyCount)
        {
            using var container = new Container();
            container.Bind<Dependency>().FromInstance(new Dependency());
            var target = CreateInjectionTarget(dependencyCount);
            AllocationMeasurement.Run("Inject", () => container.Inject(target), expectZero: dependencyCount == 0);
        }

        [Test, Performance]
        public void Inject_DirectMethodBaseline()
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            var target = new InjectionTarget();
            AllocationMeasurement.Run("DirectMethod", () => target.Construct(dependency), expectZero: true);
            AllocationMeasurement.Run("Inject.OneDependency", () => container.Inject(target));
            Assert.That(target.Dependency, Is.SameAs(dependency));
        }

        [Test, Performance]
        public void Inject_InheritedMethod_AfterWarmup()
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            var target = new InheritedInjectionTarget();
            AllocationMeasurement.Run("Inject.Inherited", () => container.Inject(target));
            Assert.That(target.Dependency, Is.SameAs(dependency));
        }

        [Test, Performance]
        public void Inject_WithTransientDependencyChain([Values(1, 5, 10)] int dependencyCount)
        {
            using var container = new Container();
            var types = ChainTypes(dependencyCount);
            foreach (var type in types) container.Bind(type).AsTransient();
            var target = Activator.CreateInstance(typeof(ChainInjectionTarget<>).MakeGenericType(types[types.Length - 1]));
            AllocationMeasurement.Run("Inject.TransientChain", () => container.Inject(target));
        }

        [Test, Performance]
        public void Inject_Enumerable([Values(1, 10, 100)] int objectCount, [Values] bool list)
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            var targets = new object[objectCount];
            for (var i = 0; i < targets.Length; i++) targets[i] = new InjectionTarget();
            IEnumerable<object> enumerable = list ? new List<object>(targets) : targets;
            AllocationMeasurement.Run("Inject.PerTarget", () => container.Inject(enumerable), operationsPerIteration: objectCount);
            foreach (InjectionTarget target in targets) Assert.That(target.Dependency, Is.SameAs(dependency));
        }

        [Test, Performance]
        public void AddToInjectionQueue_PreparedArray([Values(1, 10, 100, 1000)] int objectCount)
        {
            var targets = new object[objectCount];
            for (var i = 0; i < targets.Length; i++) targets[i] = new object();
            Container container = null;
            AllocationMeasurement.Run("InjectionQueue.Batch", () => container.AddToInjectionQueue(targets),
                setUp: () => container = new Container(), cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void AddToInjectionQueue_WithExistingObjects([Values(0, 10, 100, 1000)] int queueSize)
        {
            var targets = new object[queueSize];
            for (var i = 0; i < targets.Length; i++) targets[i] = new object();
            var added = new object();
            Container container = null;
            AllocationMeasurement.Run("InjectionQueue.Add", () => container.AddToInjectionQueue(added),
                setUp: () =>
                {
                    container = new Container();
                    container.AddToInjectionQueue(targets);
                }, cleanUp: () => container?.Dispose(), iterations: 1);
        }
    }
}
