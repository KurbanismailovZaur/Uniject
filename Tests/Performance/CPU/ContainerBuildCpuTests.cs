using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerBuildCpuTests
    {
        [Test, Performance]
        public void Build_EmptyContainer_FirstBuild()
        {
            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Build_AlreadyBuiltContainer()
        {
            using var container = new Container();
            container.Build();

            Measure.Method(() => container.Build())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Build_WithLazyBindings(int bindingCount)
        {
            var types = CreateTypes(typeof(Service<,,>), bindingCount);
            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var type in types)
                        container.Bind(type).AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [Performance]
        public void Build_WithNonLazyBindings(int objectCount, bool cached)
        {
            var types = CreateTypes(typeof(Service<,,>), objectCount);
            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var type in types)
                    {
                        var binding = container.Bind(type);
                        if (cached)
                            binding.AsCached().NonLazy();
                        else
                            binding.AsTransient().NonLazy();
                    }
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void Build_WithInjectionQueue(int objectCount)
        {
            var targets = new object[objectCount];
            for (var i = 0; i < targets.Length; i++)
                targets[i] = new InjectionTarget();

            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Dependency>().AsCached();
                    container.Resolve<Dependency>();
                    container.AddToInjectionQueue(targets);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void Build_WithEntryPoints(int entryPointCount)
        {
            var types = CreateTypes(typeof(EntryPoint<,,>), entryPointCount);
            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var type in types)
                        container.Bind(type).AsEntryPoint();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Build_WithNonLazyDependenciesInjectionQueueAndEntryPoint()
        {
            var target = new CombinedInjectionTarget();
            Container container = null;

            Measure.Method(() => container.Build())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Dependency>().AsCached().NonLazy();
                    container.Bind<DependentService>().AsCached().NonLazy();
                    container.AddToInjectionQueue(target);
                    container.Bind<DependentEntryPoint>().AsEntryPoint();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        private static Type[] CreateTypes(Type genericType, int count)
        {
            var arguments = new[]
            {
                typeof(object), typeof(string), typeof(Type), typeof(Exception), typeof(Attribute),
                typeof(Delegate), typeof(Container), typeof(IObjectBuilder), typeof(IEntryPoint), typeof(Dependency)
            };
            var types = new Type[count];
            for (var i = 0; i < types.Length; i++)
                types[i] = genericType.MakeGenericType(arguments[i / 100], arguments[i / 10 % 10], arguments[i % 10]);

            return types;
        }

        private sealed class Service<T1, T2, T3>
        {
            [Preserve]
            public Service() { }
        }

        private sealed class EntryPoint<T1, T2, T3> : IEntryPoint
        {
            [Preserve]
            public EntryPoint() { }

            public void Run() { }
        }

        private sealed class Dependency
        {
            [Preserve]
            public Dependency() { }
        }

        private sealed class InjectionTarget
        {
            [Inject]
            private void Construct(Dependency dependency) { }
        }

        private sealed class DependentService
        {
            [Preserve]
            public DependentService(Dependency dependency) { }
        }

        private sealed class CombinedInjectionTarget
        {
            [Inject]
            private void Construct(DependentService service) { }
        }

        private sealed class DependentEntryPoint : IEntryPoint
        {
            [Preserve]
            public DependentEntryPoint(DependentService service) { }

            public void Run() { }
        }
    }
}
