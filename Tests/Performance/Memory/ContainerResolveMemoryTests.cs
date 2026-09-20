using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerResolveMemoryTests
    {
        [Test, Performance]
        public void Resolve_AsCached_AfterWarmup([Values] bool generic)
        {
            using var container = new Container();
            container.Bind<Service>().AsCached();
            var expected = container.Resolve<Service>();
            object result = null;
            Action resolve = generic ? () => result = container.Resolve<Service>() : () => result = container.Resolve(typeof(Service));
            AllocationMeasurement.Run("Resolve.AsCached", resolve, expectZero: true);
            Assert.That(result, Is.SameAs(expected));
        }

        [Test, Performance]
        public void Resolve_FromInstance_AfterWarmup([Values] bool generic)
        {
            using var container = new Container();
            var expected = new Service();
            container.Bind<Service>().FromInstance(expected);
            object result = null;
            Action resolve = generic ? () => result = container.Resolve<Service>() : () => result = container.Resolve(typeof(Service));
            AllocationMeasurement.Run("Resolve.FromInstance", resolve, expectZero: true);
            Assert.That(result, Is.SameAs(expected));
        }

        [Test, Performance]
        public void Resolve_ValueTypeFromInstance_AfterWarmup([Values] bool generic)
        {
            using var container = new Container();
            container.Bind<int>().FromInstance(42).AsCached();
            var result = 0;
            // Keep the destination strongly typed so the benchmark itself does not box the result.
            Action resolve = generic ? () => result = container.Resolve<int>() : () => result = (int)container.Resolve(typeof(int));
            AllocationMeasurement.Run("Resolve.ValueInstance", resolve, expectZero: true);
            Assert.That(result, Is.EqualTo(42));
        }

        [Test, Performance]
        public void Resolve_ValueTypeFromMethod_AsTransient()
        {
            using var container = new Container();
            container.Bind<int>().FromMethod(_ => 42).AsTransient();
            var result = 0;
            AllocationMeasurement.Run("Resolve.ValueMethod", () => result = container.Resolve<int>());
            Assert.That(result, Is.EqualTo(42));
        }

        [Test, Performance]
        public void Resolve_AsCached_FromAncestor([Values(1, 4, 16)] int depth, [Values] bool generic)
        {
            var hierarchy = Hierarchy(new Container(), depth);
            try
            {
                hierarchy[0].Bind<Service>().AsCached();
                var expected = hierarchy[0].Resolve<Service>();
                var leaf = hierarchy[depth];
                object result = null;
                Action resolve = generic ? () => result = leaf.Resolve<Service>() : () => result = leaf.Resolve(typeof(Service));
                AllocationMeasurement.Run("Resolve.Ancestor", resolve, expectZero: true);
                Assert.That(result, Is.SameAs(expected));
            }
            finally { DisposeHierarchy(hierarchy); }
        }

        [Test, Performance]
        public void Resolve_AsCached_FirstResolveWithWarmReflection(
            [Values(0, 1, 3, 10)] int dependencyCount, [Values] bool preResolveDependencies)
        {
            var type = ServiceType(dependencyCount);
            using var warmup = new Container();
            warmup.Bind<Dependency>().AsCached();
            warmup.Instantiate(type);
            Container container = null;
            object result = null;
            AllocationMeasurement.Run("Resolve.FirstCached", () => result = container.Resolve(type),
                setUp: () =>
                {
                    container = new Container();
                    container.Bind<Dependency>().AsCached();
                    if (preResolveDependencies) container.Resolve<Dependency>();
                    container.Bind(type).AsCached();
                },
                cleanUp: () => container?.Dispose(), iterations: 1);
            Assert.That(result, Is.TypeOf(type));
        }

        [Test, Performance]
        public void Resolve_AsTransient_WithCachedDependencies([Values(0, 1, 3, 10)] int dependencyCount)
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            var type = ServiceType(dependencyCount);
            container.Bind(type).AsTransient();
            object result = null;
            var direct = DirectConstructor(dependencyCount, dependency);
            AllocationMeasurement.Run("DirectNew", () => result = direct());
            AllocationMeasurement.Run("Resolve.Transient", () => result = container.Resolve(type));
            Assert.That(result, Is.TypeOf(type));
        }

        [Test, Performance]
        public void Resolve_AsTransient_ChainIncludingRoot([Values(1, 5, 10)] int objectCount)
        {
            using var container = new Container();
            var types = ChainTypes(objectCount);
            foreach (var type in types) container.Bind(type).AsTransient();
            var root = types[types.Length - 1];
            object result = null;
            AllocationMeasurement.Run("Resolve.TransientChain", () => result = container.Resolve(root));
            Assert.That(result, Is.TypeOf(root));
        }

        [Test, Performance]
        public void Resolve_FromMethod_WithDependency([Values] bool cached)
        {
            using var container = new Container();
            container.Bind<Dependency>().AsCached();
            var binding = container.Bind<Service1>().FromMethod(context => new Service1(context.Container.Resolve<Dependency>()));
            if (cached) binding.AsCached(); else binding.AsTransient();
            Service1 result = null;
            AllocationMeasurement.Run("Resolve.FromMethod", () => result = container.Resolve<Service1>(), expectZero: cached);
            Assert.That(result.Dependency, Is.SameAs(container.Resolve<Dependency>()));
        }

        [Test, Performance]
        public void Resolve_FromResolve_Alias([Values] bool cachedSource)
        {
            using var container = new Container();
            if (cachedSource) container.Bind<Service>().AsCached(); else container.Bind<Service>().AsTransient();
            container.Bind<IService>().To<Service>().FromResolve().AsTransient();
            IService result = null;
            AllocationMeasurement.Run("Resolve.Alias", () => result = container.Resolve<IService>(), expectZero: cachedSource);
            Assert.That(result, Is.TypeOf<Service>());
        }

        [Test, Performance]
        public void Resolve_FromResolveGetter_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<ServiceSource>().AsCached();
            var expected = container.Resolve<ServiceSource>().Service;
            container.Bind<Service>().FromResolveGetter<ServiceSource>(source => source.Service).AsTransient();
            Service result = null;
            AllocationMeasurement.Run("Resolve.Getter", () => result = container.Resolve<Service>(), expectZero: true);
            Assert.That(result, Is.SameAs(expected));
        }

        [Test, Performance]
        public void Resolve_DisposeWithContainer([Values] bool firstResolve)
        {
            using var warmup = new Container();
            warmup.Instantiate<DisposableService>();
            Container container = null;
            DisposableService result = null;
            AllocationMeasurement.Run("Resolve.Disposable", () => result = container.Resolve<DisposableService>(),
                setUp: () =>
                {
                    container = new Container();
                    container.Bind<DisposableService>().AsCached().DisposeWithContainer();
                    if (!firstResolve) container.Resolve<DisposableService>();
                }, cleanUp: () => container?.Dispose(), iterations: 1, expectZero: !firstResolve);
            Assert.That(result.DisposeCount, Is.EqualTo(1));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        [Performance]
        public void Resolve_FromSubcontainer_AfterWarmup(bool byInstaller, bool cachedService)
        {
            using var container = new Container();
            var binding = container.Bind<Service>().FromSubcontainerResolve();
            if (byInstaller)
                binding.ByInstaller<ServiceInstaller>().AsCached();
            else
                binding.ByMethod(child =>
                {
                    if (cachedService) child.Bind<Service>().AsCached(); else child.Bind<Service>().AsTransient();
                }).AsCached();
            Service result = null;
            AllocationMeasurement.Run("Resolve.Subcontainer", () => result = container.Resolve<Service>(),
                expectZero: byInstaller || cachedService);
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Resolve_FromSubcontainerByInstance_AfterWarmup([Values] bool cachedService)
        {
            using var container = new Container();
            using var child = new Container();
            if (cachedService) child.Bind<Service>().AsCached(); else child.Bind<Service>().AsTransient();
            container.Bind<Service>().FromSubcontainerResolve().ByInstance(child).AsCached();
            Service result = null;
            AllocationMeasurement.Run("Resolve.ExistingSubcontainer", () => result = container.Resolve<Service>(), expectZero: cachedService);
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Resolve_FromSubcontainer_FirstResolveWithWarmReflection([Values] bool cachedSubcontainer)
        {
            using var warmup = new Container();
            warmup.Instantiate<Service>();
            Container container = null;
            Service result = null;
            AllocationMeasurement.Run("Resolve.NewSubcontainer", () => result = container.Resolve<Service>(),
                setUp: () =>
                {
                    container = new Container();
                    var binding = container.Bind<Service>().FromSubcontainerResolve().ByMethod(child => child.Bind<Service>().AsCached());
                    if (cachedSubcontainer) binding.AsCached(); else binding.AsTransient();
                }, cleanUp: () => container?.Dispose(), iterations: 1);
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Resolve_AsCached_WithUserBindings([Values(1, 10, 100, 1000)] int bindingCount)
        {
            using var container = new Container();
            foreach (var type in CreateTypes(typeof(Contract<,,>), bindingCount - 1)) container.Bind(type).AsCached();
            container.Bind<Service>().AsCached();
            var expected = container.Resolve<Service>();
            Service result = null;
            AllocationMeasurement.Run("Resolve.BindingCount", () => result = container.Resolve<Service>(), expectZero: true);
            Assert.That(result, Is.SameAs(expected));
        }
    }
}
