using NUnit.Framework;
using Uniject;
using Uniject.Bindings;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ContainerResolvePerformanceTests
    {
        private const int HotIterationsPerMeasurement = 10_000;
        private const int GraphIterationsPerMeasurement = 1_000;
        private const int SubcontainerResolvesPerMeasurement = 100;
        private const int BindingsPerMeasurement = 1_000;
        private const int WarmupCount = 5;
        private const int MeasurementCount = 20;

        private static Foo _resolvedFoo;
        private static GraphRoot _resolvedGraphRoot;
        private static SubcontainerProduct _resolvedSubcontainerProduct;
        private static object _resolvedObject;
        private static object _bindingSink;
        private static bool _tryResolveSucceeded;

        [Preserve]
        private sealed class Foo
        {
            [Preserve]
            public Foo() { }
        }

        [Preserve]
        private sealed class GraphLeafA
        {
            [Preserve]
            public GraphLeafA() { }
        }

        [Preserve]
        private sealed class GraphLeafB
        {
            [Preserve]
            public GraphLeafB() { }
        }

        [Preserve]
        private sealed class GraphLeafC
        {
            [Preserve]
            public GraphLeafC() { }
        }

        [Preserve]
        private sealed class GraphLeafD
        {
            [Preserve]
            public GraphLeafD() { }
        }

        [Preserve]
        private sealed class GraphBranchA
        {
            public GraphLeafA LeafA { get; }
            public GraphLeafB LeafB { get; }

            [Preserve]
            public GraphBranchA(GraphLeafA leafA, GraphLeafB leafB)
            {
                LeafA = leafA;
                LeafB = leafB;
            }
        }

        [Preserve]
        private sealed class GraphBranchB
        {
            public GraphLeafC LeafC { get; }
            public GraphLeafD LeafD { get; }

            [Preserve]
            public GraphBranchB(GraphLeafC leafC, GraphLeafD leafD)
            {
                LeafC = leafC;
                LeafD = leafD;
            }
        }

        [Preserve]
        private sealed class GraphRoot
        {
            public GraphBranchA BranchA { get; }
            public GraphBranchB BranchB { get; }

            [Preserve]
            public GraphRoot(GraphBranchA branchA, GraphBranchB branchB)
            {
                BranchA = branchA;
                BranchB = branchB;
            }
        }

        [Preserve]
        private sealed class GraphRootFactory : Factory<GraphRoot>
        {
            [Preserve]
            public GraphRootFactory() { }
        }

        [Preserve]
        private sealed class MissingBinding
        {
            [Preserve]
            public MissingBinding() { }
        }

        [Preserve]
        private sealed class SubcontainerLeaf
        {
            [Preserve]
            public SubcontainerLeaf() { }
        }

        [Preserve]
        private sealed class SubcontainerProduct
        {
            public SubcontainerLeaf Leaf { get; }

            [Preserve]
            public SubcontainerProduct(SubcontainerLeaf leaf)
            {
                Leaf = leaf;
            }
        }

        [Test, Performance, Version("1")]
        public void ResolveCached_FromConstructor_MeasuresHotResolveTimeAndAllocations()
        {
            using var container = new Container();
            container.Bind<Foo>().FromConstructor().AsCached();
            container.Build();

            _resolvedFoo = container.Resolve<Foo>();

            Measure.Method(() => _resolvedFoo = container.Resolve<Foo>())
                .SampleGroup(new SampleGroup(
                    "ResolveCached.FromConstructor.10000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(HotIterationsPerMeasurement)
                .GC()
                .Run();
        }

        [Test, Performance, Version("1")]
        public void ResolveTransient_FromConstructor_MeasuresHotResolveTimeAndAllocations()
        {
            using var container = new Container();
            container.Bind<Foo>().FromConstructor().AsTransient();
            container.Build();

            _resolvedFoo = container.Resolve<Foo>();

            Measure.Method(() => _resolvedFoo = container.Resolve<Foo>())
                .SampleGroup(new SampleGroup(
                    "ResolveTransient.FromConstructor.10000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(HotIterationsPerMeasurement)
                .GC()
                .Run();
        }

        [Test, Performance, Version("1")]
        public void ResolveTransient_FromConstructorGraph7_MeasuresHotResolveTimeAndAllocations()
        {
            using var container = new Container();
            BindConstructorGraphDependencies(container);
            container.Bind<GraphRoot>().FromConstructor().AsTransient();
            container.Build();

            _resolvedGraphRoot = container.Resolve<GraphRoot>();

            Measure.Method(() => _resolvedGraphRoot = container.Resolve<GraphRoot>())
                .SampleGroup(new SampleGroup(
                    "ResolveTransient.FromConstructorGraph7.1000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(GraphIterationsPerMeasurement)
                .GC()
                .Run();
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance, Version("1")]
        public void ResolveCached_FromAncestorAtDepth_MeasuresHotResolveTimeAndAllocations(int ancestorDepth)
        {
            var containers = new Container[ancestorDepth + 1];
            containers[0] = new Container();

            try
            {
                containers[0].Bind<Foo>().FromConstructor().AsCached();
                containers[0].Build();
                _resolvedFoo = containers[0].Resolve<Foo>();

                for (var i = 1; i < containers.Length; i++)
                {
                    containers[i] = new Container(containers[i - 1]);
                    containers[i].Build();
                }

                var resolvingContainer = containers[containers.Length - 1];

                Measure.Method(() => _resolvedFoo = resolvingContainer.Resolve<Foo>())
                    .SampleGroup(new SampleGroup(
                        $"ResolveCached.AncestorDepth{ancestorDepth}.10000Calls",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(HotIterationsPerMeasurement)
                    .GC()
                    .Run();
            }
            finally
            {
                for (var i = containers.Length - 1; i >= 0; i--)
                    containers[i]?.Dispose();
            }
        }

        [Test, Performance, Version("1")]
        public void TryResolve_WhenBindingIsMissing_MeasuresFailurePathTimeAndAllocations()
        {
            using var container = new Container();
            container.Build();

            Measure.Method(() =>
                {
                    var result = container.TryResolve<MissingBinding>();
                    _resolvedObject = result.Item1;
                    _tryResolveSucceeded = result.Item2;
                })
                .SampleGroup(new SampleGroup(
                    "TryResolve.MissingBinding.1000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(GraphIterationsPerMeasurement)
                .GC()
                .Run();
        }

        [TestCase(Scope.Transient)]
        [TestCase(Scope.Cached)]
        [Performance, Version("1")]
        public void Resolve_FromSubcontainerByMethod_MeasuresScopeCostAndAllocations(Scope scope)
        {
            Container container = null;

            try
            {
                Measure.Method(() =>
                    {
                        for (var i = 0; i < SubcontainerResolvesPerMeasurement; i++)
                            _resolvedSubcontainerProduct = container.Resolve<SubcontainerProduct>();
                    })
                    .SampleGroup(new SampleGroup(
                        $"Resolve.FromSubcontainer.ByMethod.{scope}.100Calls",
                        SampleUnit.Microsecond))
                    .SetUp(() =>
                    {
                        container = CreateSubcontainerBenchmarkContainer(scope);

                        if (scope == Scope.Cached)
                            _resolvedSubcontainerProduct = container.Resolve<SubcontainerProduct>();
                    })
                    .CleanUp(() =>
                    {
                        container.Dispose();
                        container = null;
                    })
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
        public void FactoryCreate_FromConstructorGraph7_MeasuresHotCreateTimeAndAllocations()
        {
            using var container = new Container();
            BindConstructorGraphDependencies(container);
            container.BindFactory<GraphRoot, GraphRootFactory>()
                .FromConstructor()
                .AsCached();
            container.Build();

            var factory = container.Resolve<GraphRootFactory>();
            _resolvedGraphRoot = factory.Create();

            Measure.Method(() => _resolvedGraphRoot = factory.Create())
                .SampleGroup(new SampleGroup(
                    "FactoryCreate.FromConstructorGraph7.1000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(GraphIterationsPerMeasurement)
                .GC()
                .Run();
        }

        [Test, Performance, Version("1")]
        public void Bind_FromConstructorAsCached_MeasuresRegistrationTimeAndAllocations()
        {
            Container[] containers = null;

            try
            {
                Measure.Method(() =>
                    {
                        for (var i = 0; i < containers.Length; i++)
                        {
                            _bindingSink = containers[i]
                                .Bind<Foo>()
                                .FromConstructor()
                                .AsCached();
                        }
                    })
                    .SampleGroup(new SampleGroup(
                        "Bind.FromConstructor.AsCached.1000Bindings",
                        SampleUnit.Microsecond))
                    .SetUp(() =>
                    {
                        containers = new Container[BindingsPerMeasurement];

                        for (var i = 0; i < containers.Length; i++)
                            containers[i] = new Container();
                    })
                    .CleanUp(() =>
                    {
                        DisposeContainers(containers);
                        containers = null;
                    })
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                DisposeContainers(containers);
            }
        }

        private static void DisposeContainers(Container[] containers)
        {
            if (containers == null)
                return;

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i]?.Dispose();
        }

        private static void BindConstructorGraphDependencies(Container container)
        {
            container.Bind<GraphLeafA>().FromConstructor().AsTransient();
            container.Bind<GraphLeafB>().FromConstructor().AsTransient();
            container.Bind<GraphLeafC>().FromConstructor().AsTransient();
            container.Bind<GraphLeafD>().FromConstructor().AsTransient();
            container.Bind<GraphBranchA>().FromConstructor().AsTransient();
            container.Bind<GraphBranchB>().FromConstructor().AsTransient();
        }

        private static Container CreateSubcontainerBenchmarkContainer(Scope scope)
        {
            var container = new Container();
            var scopeBuilder = container.Bind<SubcontainerProduct>()
                .FromSubcontainerResolve()
                .ByMethod(subcontainer =>
                {
                    subcontainer.Bind<SubcontainerLeaf>().FromConstructor().AsCached();
                    subcontainer.Bind<SubcontainerProduct>().FromConstructor().AsTransient();
                });

            if (scope == Scope.Cached)
                scopeBuilder.AsCached();
            else
                scopeBuilder.AsTransient();

            container.Build();
            return container;
        }
    }
}
