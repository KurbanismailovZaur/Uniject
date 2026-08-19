using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerTryResolveCpuTests
    {
        [Test, Performance]
        public void TryResolveGeneric_AsCached_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<Service>().AsCached();
            container.TryResolve<Service>();

            Measure.Method(() => container.TryResolve<Service>())
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void TryResolveType_AsCached_AfterWarmup()
        {
            using var container = new Container();
            var serviceType = typeof(Service);
            container.Bind<Service>().AsCached();
            container.TryResolve(serviceType);

            Measure.Method(() => container.TryResolve(serviceType))
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void TryResolveGeneric_AsTransient_WithoutDependencies()
        {
            using var container = new Container();
            container.Bind<Service>().AsTransient();
            container.TryResolve<Service>();

            Measure.Method(() => container.TryResolve<Service>())
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void TryResolveType_AsTransient_WithoutDependencies()
        {
            using var container = new Container();
            var serviceType = typeof(Service);
            container.Bind<Service>().AsTransient();
            container.TryResolve(serviceType);

            Measure.Method(() => container.TryResolve(serviceType))
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void TryResolveGeneric_AsCached_FromAncestor(int depth)
        {
            var containers = CreateHierarchy(depth);
            containers[0].Bind<Service>().AsCached();
            var leaf = containers[depth];
            leaf.TryResolve<Service>();

            Measure.Method(() => leaf.TryResolve<Service>())
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i].Dispose();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void TryResolveType_AsCached_FromAncestor(int depth)
        {
            var containers = CreateHierarchy(depth);
            var serviceType = typeof(Service);
            containers[0].Bind<Service>().AsCached();
            var leaf = containers[depth];
            leaf.TryResolve(serviceType);

            Measure.Method(() => leaf.TryResolve(serviceType))
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i].Dispose();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void TryResolveGeneric_MissingBinding(int depth)
        {
            var containers = CreateHierarchy(depth);
            var leaf = containers[depth];
            leaf.TryResolve<Service>();

            Measure.Method(() => leaf.TryResolve<Service>())
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i].Dispose();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void TryResolveType_MissingBinding(int depth)
        {
            var containers = CreateHierarchy(depth);
            var serviceType = typeof(Service);
            var leaf = containers[depth];
            leaf.TryResolve(serviceType);

            Measure.Method(() => leaf.TryResolve(serviceType))
                .SampleGroup(new SampleGroup("TryResolve", SampleUnit.Nanosecond))
                .Run();

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i].Dispose();
        }

        private static Container[] CreateHierarchy(int depth)
        {
            var containers = new Container[depth + 1];
            containers[0] = new Container();

            for (var i = 1; i < containers.Length; i++)
                containers[i] = new Container(containers[i - 1]);

            return containers;
        }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }
    }
}
