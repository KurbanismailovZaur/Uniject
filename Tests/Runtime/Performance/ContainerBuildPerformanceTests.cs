using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ContainerBuildPerformanceTests
    {
        private const int WarmupCount = 3;
        private const int MeasurementCount = 20;

        private static readonly System.Type[] ContractTypes =
        {
            typeof(IContract01),
            typeof(IContract02),
            typeof(IContract03),
            typeof(IContract04),
            typeof(IContract05),
            typeof(IContract06),
            typeof(IContract07),
            typeof(IContract08),
            typeof(IContract09),
            typeof(IContract10),
            typeof(IContract11),
            typeof(IContract12),
            typeof(IContract13),
            typeof(IContract14),
            typeof(IContract15),
            typeof(IContract16),
            typeof(IContract17),
            typeof(IContract18),
            typeof(IContract19),
            typeof(IContract20),
            typeof(IContract21),
            typeof(IContract22),
            typeof(IContract23),
            typeof(IContract24),
            typeof(IContract25),
            typeof(IContract26),
            typeof(IContract27),
            typeof(IContract28),
            typeof(IContract29),
            typeof(IContract30),
            typeof(IContract31),
            typeof(IContract32)
        };

        [TestCase(32)]
        [TestCase(256)]
        [TestCase(1024)]
        [Performance, Version("1")]
        public void Build_QueuedInjectables_ScalesWithTargetCount(int targetCount)
        {
            Container container = null;

            try
            {
                Measure.Method(() => container.Build())
                    .SetUp(() =>
                    {
                        container = new Container();
                        container.Bind<QueuedDependency>().FromInstance(new QueuedDependency()).AsCached();

                        for (var i = 0; i < targetCount; i++)
                            container.AddToInjectionQueue(new QueuedInjectable());
                    })
                    .CleanUp(() =>
                    {
                        container.Dispose();
                        container = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Container.Build.QueuedInjectables.{targetCount}Targets.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                container?.Dispose();
            }
        }

        [TestCase(1)]
        [TestCase(8)]
        [TestCase(32)]
        [Performance, Version("1")]
        public void Build_NonLazyBindings_ScalesWithBindingCount(int bindingCount)
        {
            Container container = null;

            try
            {
                Measure.Method(() => container.Build())
                    .SetUp(() =>
                    {
                        container = new Container();

                        for (var i = 0; i < bindingCount; i++)
                        {
                            container.Bind(ContractTypes[i])
                                .To(typeof(BenchmarkService))
                                .FromConstructor()
                                .AsCached()
                                .NonLazy();
                        }
                    })
                    .CleanUp(() =>
                    {
                        container.Dispose();
                        container = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Container.Build.NonLazyBindings.{bindingCount}UserBindings.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                container?.Dispose();
            }
        }

        [Test, Performance, Version("1")]
        public void ConfigureAndBuild_Representative32Bindings_MeasuresWarmCost()
        {
            Container container = null;

            try
            {
                Measure.Method(() =>
                    {
                        ConfigureRepresentativeBindings(container);
                        container.Build();
                    })
                    .SetUp(() => container = new Container())
                    .CleanUp(() =>
                    {
                        container.Dispose();
                        container = null;
                    })
                    .SampleGroup(new SampleGroup(
                        "Container.ConfigureAndBuild.Representative.32UserBindings.Warm.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                container?.Dispose();
            }
        }

        private static void ConfigureRepresentativeBindings(Container container)
        {
            // 16 lazy transient bindings.
            for (var i = 0; i < 16; i++)
            {
                container.Bind(ContractTypes[i])
                    .To(typeof(BenchmarkService))
                    .FromConstructor()
                    .AsTransient();
            }

            // 8 cached non-lazy bindings.
            for (var i = 16; i < 24; i++)
            {
                container.Bind(ContractTypes[i])
                    .To(typeof(BenchmarkService))
                    .FromConstructor()
                    .AsCached()
                    .NonLazy();
            }

            // 4 cached entry-point bindings.
            for (var i = 24; i < 28; i++)
            {
                container.Bind(ContractTypes[i])
                    .To(typeof(BenchmarkService))
                    .FromConstructor()
                    .AsCached()
                    .NonLazy()
                    .AsEntryPoint();
            }

            // 4 cached dependency bindings complete the representative set of 32.
            container.Bind<IContract29>().FromInstance(new DependencyOne()).AsCached();
            container.Bind<IContract30>().FromInstance(new DependencyTwo()).AsCached();
            container.Bind<IContract31>().FromInstance(new DependencyThree()).AsCached();
            container.Bind<IContract32>().FromInstance(new DependencyFour()).AsCached();

            for (var i = 0; i < 4; i++)
                container.AddToInjectionQueue(new RepresentativeInjectable());
        }

        [Preserve]
        private sealed class QueuedDependency
        {
            [Preserve]
            public QueuedDependency() { }
        }

        [Preserve]
        private sealed class QueuedInjectable
        {
            private QueuedDependency _dependency;

            [Preserve]
            public QueuedInjectable() { }

            [Inject]
            public void Construct(QueuedDependency dependency) => _dependency = dependency;
        }

        [Preserve]
        private sealed class BenchmarkService :
            IContract01, IContract02, IContract03, IContract04,
            IContract05, IContract06, IContract07, IContract08,
            IContract09, IContract10, IContract11, IContract12,
            IContract13, IContract14, IContract15, IContract16,
            IContract17, IContract18, IContract19, IContract20,
            IContract21, IContract22, IContract23, IContract24,
            IContract25, IContract26, IContract27, IContract28,
            IContract29, IContract30, IContract31, IContract32,
            IEntryPoint
        {
            private static int _runs;

            [Preserve]
            public BenchmarkService() { }

            [Preserve]
            public void Run() => _runs++;
        }

        [Preserve]
        private sealed class DependencyOne : IContract29
        {
            [Preserve]
            public DependencyOne() { }
        }

        [Preserve]
        private sealed class DependencyTwo : IContract30
        {
            [Preserve]
            public DependencyTwo() { }
        }

        [Preserve]
        private sealed class DependencyThree : IContract31
        {
            [Preserve]
            public DependencyThree() { }
        }

        [Preserve]
        private sealed class DependencyFour : IContract32
        {
            [Preserve]
            public DependencyFour() { }
        }

        [Preserve]
        private sealed class RepresentativeInjectable
        {
            private IContract29 _dependencyOne;
            private IContract30 _dependencyTwo;
            private IContract31 _dependencyThree;
            private IContract32 _dependencyFour;

            [Preserve]
            public RepresentativeInjectable() { }

            [Inject]
            public void Construct(
                IContract29 dependencyOne,
                IContract30 dependencyTwo,
                IContract31 dependencyThree,
                IContract32 dependencyFour)
            {
                _dependencyOne = dependencyOne;
                _dependencyTwo = dependencyTwo;
                _dependencyThree = dependencyThree;
                _dependencyFour = dependencyFour;
            }
        }

        [Preserve] private interface IContract01 { }
        [Preserve] private interface IContract02 { }
        [Preserve] private interface IContract03 { }
        [Preserve] private interface IContract04 { }
        [Preserve] private interface IContract05 { }
        [Preserve] private interface IContract06 { }
        [Preserve] private interface IContract07 { }
        [Preserve] private interface IContract08 { }
        [Preserve] private interface IContract09 { }
        [Preserve] private interface IContract10 { }
        [Preserve] private interface IContract11 { }
        [Preserve] private interface IContract12 { }
        [Preserve] private interface IContract13 { }
        [Preserve] private interface IContract14 { }
        [Preserve] private interface IContract15 { }
        [Preserve] private interface IContract16 { }
        [Preserve] private interface IContract17 { }
        [Preserve] private interface IContract18 { }
        [Preserve] private interface IContract19 { }
        [Preserve] private interface IContract20 { }
        [Preserve] private interface IContract21 { }
        [Preserve] private interface IContract22 { }
        [Preserve] private interface IContract23 { }
        [Preserve] private interface IContract24 { }
        [Preserve] private interface IContract25 { }
        [Preserve] private interface IContract26 { }
        [Preserve] private interface IContract27 { }
        [Preserve] private interface IContract28 { }
        [Preserve] private interface IContract29 { }
        [Preserve] private interface IContract30 { }
        [Preserve] private interface IContract31 { }
        [Preserve] private interface IContract32 { }
    }
}
