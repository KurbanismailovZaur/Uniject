using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerTryResolveMemoryTests
    {
        [Test, Performance]
        public void TryResolve_ValueTypeFromInstance_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<int>().FromInstance(42).AsCached();
            (int value, bool resolved) result = default;
            AllocationMeasurement.Run("TryResolve.ValueInstance", () => result = container.TryResolve<int>(), expectZero: true);
            Assert.That(result.value, Is.EqualTo(42));
            Assert.That(result.resolved, Is.True);
        }

        [Test, Performance]
        public void TryResolve_AsCached_FromAncestor([Values(0, 1, 4, 16)] int depth, [Values] bool generic)
        {
            var hierarchy = Hierarchy(new Container(), depth);
            try
            {
                hierarchy[0].Bind<Service>().AsCached();
                var expected = hierarchy[0].Resolve<Service>();
                var leaf = hierarchy[depth];
                object result = null;
                Action resolve = generic ? () => result = leaf.TryResolve<Service>().Item1 : () => result = leaf.TryResolve(typeof(Service));
                AllocationMeasurement.Run("TryResolve.Cached", resolve, expectZero: true);
                Assert.That(result, Is.SameAs(expected));
            }
            finally { DisposeHierarchy(hierarchy); }
        }

        [Test, Performance]
        public void TryResolve_MissingBinding([Values(0, 1, 4, 16)] int depth, [Values] bool generic)
        {
            var hierarchy = Hierarchy(new Container(), depth);
            try
            {
                var leaf = hierarchy[depth];
                object result = new object();
                bool resolved = true;
                Action resolve = generic
                    ? () => { var value = leaf.TryResolve<Service>(); result = value.Item1; resolved = value.resolved; }
                    : () => { result = leaf.TryResolve(typeof(Service)); resolved = result != null; };
                AllocationMeasurement.Run("TryResolve.Missing", resolve, expectZero: true);
                Assert.That(result, Is.Null);
                Assert.That(resolved, Is.False);
            }
            finally { DisposeHierarchy(hierarchy); }
        }

        [Test, Performance]
        public void TryResolve_AsTransient([Values] bool generic)
        {
            using var container = new Container();
            container.Bind<Service>().AsTransient();
            object result = null;
            Action resolve = generic ? () => result = container.TryResolve<Service>().Item1 : () => result = container.TryResolve(typeof(Service));
            AllocationMeasurement.Run("TryResolve.Transient", resolve);
            Assert.That(result, Is.TypeOf<Service>());
        }

        [Test, Performance]
        public void TryResolve_BindingWithMissingDependency([Values] bool generic)
        {
            using var container = new Container();
            container.Bind<Service1>().AsTransient();
            object result = new object();
            Action resolve = generic ? () => result = container.TryResolve<Service1>().Item1 : () => result = container.TryResolve(typeof(Service1));
            // This goes through NoBindingFoundException; it is deliberately separate from a missing root binding.
            AllocationMeasurement.Run("TryResolve.MissingDependency", resolve, iterations: 10);
            Assert.That(result, Is.Null);
        }
    }
}
