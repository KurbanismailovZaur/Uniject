using System;
using NUnit.Framework;
using Uniject.Installers;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerDisposeCpuTests
    {
        [Test, Performance]
        public void Dispose_EmptyContainer()
        {
            Container container = null;

            Measure.Method(() => container.Dispose())
                .SetUp(() => container = new Container())
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Dispose_WithUserBindings(int bindingCount)
        {
            var types = CreateTypes(typeof(Service<,,>), bindingCount);
            Container container = null;

            Measure.Method(() => container.Dispose())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var type in types)
                        container.Bind(type).AsCached();
                })
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void Dispose_WithResolvedDisposableObjects(int objectCount)
        {
            var types = CreateTypes(typeof(DisposableService<,,>), objectCount);
            Container container = null;

            Measure.Method(() => container.Dispose())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var type in types)
                    {
                        container.Bind(type).AsCached().DisposeWithContainer();
                        container.Resolve(type);
                    }
                })
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(3, false)]
        [TestCase(1, true)]
        [TestCase(3, true)]
        [Performance]
        public void Dispose_WithOwnedSubcontainerTree(int depth, bool byInstaller)
        {
            Container container = null;

            Measure.Method(() => container.Dispose())
                .SetUp(() =>
                {
                    container = new Container();
                    PopulateOwnedTree(container, depth, byInstaller);
                })
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Dispose_WithSharedInstanceAcrossContractsAndOwnedSubcontainers()
        {
            Container container = null;

            Measure.Method(() => container.Dispose())
                .SetUp(() =>
                {
                    container = new Container();
                    var instance = new SharedDisposable();
                    BindSharedDisposable(container, instance);
                    container.Bind<IContainerHandle>().To<ContainerHandle>().FromSubcontainerResolve()
                        .ByMethod(child =>
                        {
                            InstallChild(child);
                            BindSharedDisposable(child, instance);
                        }).AsTransient();
                    container.Resolve<IContainerHandle>();
                    container.Resolve<IContainerHandle>();
                })
                .SampleGroup(new SampleGroup("Dispose", SampleUnit.Nanosecond))
                .Run();
        }

        private static void PopulateOwnedTree(Container container, int depth, bool byInstaller)
        {
            if (depth == 0)
                return;

            var binding = container.Bind<IContainerHandle>().To<ContainerHandle>().FromSubcontainerResolve();
            if (byInstaller)
                binding.ByInstaller<ChildInstaller>().AsTransient();
            else
                binding.ByMethod(InstallChild).AsTransient();

            for (var i = 0; i < 2; i++)
                PopulateOwnedTree(container.Resolve<IContainerHandle>().Container, depth - 1, byInstaller);
        }

        private static void InstallChild(Container container)
        {
            container.Bind<ContainerHandle>().FromInstance(new ContainerHandle(container)).AsCached();
        }

        private static void BindSharedDisposable(Container container, SharedDisposable instance)
        {
            container.Bind<SharedDisposable>().FromInstance(instance).AsCached().DisposeWithContainer();
            container.Bind<ISharedServiceA>().FromInstance(instance).AsCached().DisposeWithContainer();
            container.Bind<ISharedServiceB>().FromInstance(instance).AsCached().DisposeWithContainer();
            container.Resolve<SharedDisposable>();
            container.Resolve<ISharedServiceA>();
            container.Resolve<ISharedServiceB>();
        }

        private interface IContainerHandle
        {
            Container Container { get; }
        }

        private sealed class ContainerHandle : IContainerHandle
        {
            public Container Container { get; }

            public ContainerHandle(Container container) => Container = container;
        }

        private sealed class ChildInstaller : IInstaller
        {
            [Preserve]
            public ChildInstaller() { }

            public void Install(Container container) => InstallChild(container);
        }

        private interface ISharedServiceA { }
        private interface ISharedServiceB { }

        private sealed class SharedDisposable : ISharedServiceA, ISharedServiceB, IDisposable
        {
            public void Dispose() { }
        }

        private static Type[] CreateTypes(Type genericType, int count)
        {
            var arguments = new[]
            {
                typeof(object), typeof(string), typeof(Type), typeof(Exception), typeof(Attribute),
                typeof(Delegate), typeof(Container), typeof(IObjectBuilder), typeof(IDisposable), typeof(IInstaller)
            };
            var types = new Type[count];
            for (var i = 0; i < types.Length; i++)
                types[i] = genericType.MakeGenericType(arguments[i / 100], arguments[i / 10 % 10], arguments[i % 10]);

            return types;
        }

        private sealed class Service<T1, T2, T3> { }

        private sealed class DisposableService<T1, T2, T3> : IDisposable
        {
            [Preserve]
            public DisposableService() { }

            public void Dispose() { }
        }
    }
}
