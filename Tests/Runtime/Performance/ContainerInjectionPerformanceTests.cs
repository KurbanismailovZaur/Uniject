using NUnit.Framework;
using Uniject.Attributes;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ContainerInjectionPerformanceTests
    {
        private const int IterationsPerMeasurement = 1_000;
        private const int WarmupCount = 5;
        private const int MeasurementCount = 20;

        private static object _sink;

        [Test, Performance, Version("1")]
        public void Inject_HotWithFourCachedDependencies_MeasuresTimeAndAllocations()
        {
            using var container = CreateContainerWithCachedDependencies();
            var target = new InjectableTarget();

            container.Inject(target);

            Measure.Method(() =>
                {
                    container.Inject(target);
                    _sink = target;
                })
                .SampleGroup(new SampleGroup(
                    "Container.Inject.Hot.FourCachedDependencies.1000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .GC()
                .Run();
        }

        [Test, Performance, Version("1")]
        public void Instantiate_HotWithFourCachedDependencies_MeasuresTimeAndAllocations()
        {
            using var container = CreateContainerWithCachedDependencies();

            _sink = container.Instantiate<InstantiatedObject>();

            Measure.Method(() => _sink = container.Instantiate<InstantiatedObject>())
                .SampleGroup(new SampleGroup(
                    "Container.Instantiate.Hot.FourCachedDependencies.1000Calls",
                    SampleUnit.Microsecond))
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .GC()
                .Run();
        }

        private static Container CreateContainerWithCachedDependencies()
        {
            var container = new Container();
            container.Bind<DependencyOne>().FromInstance(new DependencyOne()).AsCached();
            container.Bind<DependencyTwo>().FromInstance(new DependencyTwo()).AsCached();
            container.Bind<DependencyThree>().FromInstance(new DependencyThree()).AsCached();
            container.Bind<DependencyFour>().FromInstance(new DependencyFour()).AsCached();
            container.Build();
            return container;
        }

        [Preserve]
        private sealed class DependencyOne
        {
            [Preserve]
            public DependencyOne() { }
        }

        [Preserve]
        private sealed class DependencyTwo
        {
            [Preserve]
            public DependencyTwo() { }
        }

        [Preserve]
        private sealed class DependencyThree
        {
            [Preserve]
            public DependencyThree() { }
        }

        [Preserve]
        private sealed class DependencyFour
        {
            [Preserve]
            public DependencyFour() { }
        }

        [Preserve]
        private sealed class InjectableTarget
        {
            private DependencyOne _dependencyOne;
            private DependencyTwo _dependencyTwo;
            private DependencyThree _dependencyThree;
            private DependencyFour _dependencyFour;

            [Preserve]
            public InjectableTarget() { }

            [Inject]
            public void Construct(
                DependencyOne dependencyOne,
                DependencyTwo dependencyTwo,
                DependencyThree dependencyThree,
                DependencyFour dependencyFour)
            {
                _dependencyOne = dependencyOne;
                _dependencyTwo = dependencyTwo;
                _dependencyThree = dependencyThree;
                _dependencyFour = dependencyFour;
            }
        }

        [Preserve]
        private sealed class InstantiatedObject
        {
            private readonly DependencyOne _dependencyOne;
            private readonly DependencyTwo _dependencyTwo;
            private readonly DependencyThree _dependencyThree;
            private readonly DependencyFour _dependencyFour;

            [Inject]
            public InstantiatedObject(
                DependencyOne dependencyOne,
                DependencyTwo dependencyTwo,
                DependencyThree dependencyThree,
                DependencyFour dependencyFour)
            {
                _dependencyOne = dependencyOne;
                _dependencyTwo = dependencyTwo;
                _dependencyThree = dependencyThree;
                _dependencyFour = dependencyFour;
            }
        }
    }
}
