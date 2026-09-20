using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerDisposeMemoryTests
    {
        [Test, Performance]
        public void Dispose_WithUserBindings([Values(0, 10, 100, 1000)] int bindingCount, [Values] bool resolved)
        {
            var types = CreateTypes(typeof(Contract<,,>), bindingCount);
            Container container = null;
            AllocationMeasurement.Run("Dispose.Bindings", () => container.Dispose(), setUp: () =>
            {
                container = new Container();
                foreach (var type in types)
                {
                    container.Bind(type).AsCached();
                    if (resolved) container.Resolve(type);
                }
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Dispose_WithDisposableInstances([Values(1, 10, 100, 1000)] int objectCount)
        {
            var types = CreateTypes(typeof(DisposableContract<,,>), objectCount);
            Container container = null;
            AllocationMeasurement.Run("Dispose.Disposables", () => container.Dispose(), setUp: () =>
            {
                container = new Container();
                foreach (var type in types)
                {
                    container.Bind(type).AsCached().DisposeWithContainer();
                    container.Resolve(type);
                }
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Dispose_WithUnprocessedInjectionQueue([Values(1, 100, 1000)] int objectCount)
        {
            var targets = new object[objectCount];
            for (var i = 0; i < targets.Length; i++) targets[i] = new object();
            Container container = null;
            AllocationMeasurement.Run("Dispose.InjectionQueue", () => container.Dispose(), setUp: () =>
            {
                container = new Container();
                container.AddToInjectionQueue(targets);
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Dispose_WithOwnedSubcontainerTree([Values(1, 3)] int depth)
        {
            Container container = null;
            AllocationMeasurement.Run("Dispose.OwnedTree", () => container.Dispose(), setUp: () =>
            {
                container = new Container();
                PopulateTree(container, depth);
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Dispose_SharedInstanceAcrossContractsAndSubcontainers()
        {
            Container container = null;
            DisposableService service = null;
            AllocationMeasurement.Run("Dispose.SharedInstance", () => container.Dispose(), setUp: () =>
            {
                container = new Container();
                service = new DisposableService();
                BindShared(container, service);
                container.Bind<IContainerHandle>().To<ContainerHandle>().FromSubcontainerResolve().ByMethod(child =>
                {
                    child.Bind<ContainerHandle>().AsCached();
                    BindShared(child, service);
                }).AsTransient();
                container.Resolve<IContainerHandle>();
                container.Resolve<IContainerHandle>();
            }, cleanUp: () =>
            {
                container?.Dispose();
                Assert.That(service.DisposeCount, Is.EqualTo(1));
            }, iterations: 1);
        }

        [Test, Performance]
        public void Dispose_WithThrowingDisposable()
        {
            Container container = null;
            int exceptionCount = 0;
            AllocationMeasurement.Run("Dispose.Exception", () =>
            {
                try { container.Dispose(); }
                catch (AggregateException exception) { exceptionCount = exception.InnerExceptions.Count; }
            }, setUp: () =>
            {
                exceptionCount = 0;
                container = new Container();
                container.Bind<ThrowingDisposable>().AsCached().DisposeWithContainer();
                container.Resolve<ThrowingDisposable>();
            }, cleanUp: () =>
            {
                container?.Dispose();
                Assert.That(exceptionCount, Is.EqualTo(1));
            }, iterations: 1);
        }

        [Test, Performance]
        public void Dispose_AlreadyDisposedContainer()
        {
            var container = new Container();
            container.Dispose();
            // The public Dispose path currently creates a HashSet even for an already disposed container.
            AllocationMeasurement.Run("Dispose.Repeated", () => container.Dispose());
        }

        private static void PopulateTree(Container container, int depth)
        {
            if (depth == 0) return;
            container.Bind<IContainerHandle>().To<ContainerHandle>().FromSubcontainerResolve()
                .ByMethod(child => child.Bind<ContainerHandle>().AsCached()).AsTransient();
            for (var i = 0; i < 2; i++) PopulateTree(container.Resolve<IContainerHandle>().Container, depth - 1);
        }

        private static void BindShared(Container container, DisposableService service)
        {
            container.Bind<DisposableService>().FromInstance(service).AsCached().DisposeWithContainer();
            container.Bind<IService>().FromInstance(service).AsCached().DisposeWithContainer();
        }

        public sealed class DisposableContract<T1, T2, T3> : IDisposable
        {
            [Preserve] public DisposableContract() { }
            public void Dispose() { }
        }

        public sealed class ThrowingDisposable : IDisposable
        {
            [Preserve] public ThrowingDisposable() { }
            public void Dispose() => throw new InvalidOperationException("Expected memory-test disposal failure.");
        }
    }
}
