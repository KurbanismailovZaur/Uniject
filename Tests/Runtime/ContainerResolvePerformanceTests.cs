using NUnit.Framework;
using Uniject;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ContainerResolvePerformanceTests
    {
        private const int IterationsPerMeasurement = 10_000;
        private static Foo _resolvedFoo;

        [Preserve]
        private sealed class Foo
        {
            [Preserve]
            public Foo() { }
        }

        [Test, Performance, Version("1")]
        public void ResolveCached_FromConstructor_MeasuresHotResolveTime()
        {
            using var container = new Container();
            container.Bind<Foo>().FromConstructor().AsCached();
            container.Build();

            _resolvedFoo = container.Resolve<Foo>();

            var sampleGroup = new SampleGroup(
                "ResolveCached.FromConstructor.10000Calls",
                SampleUnit.Microsecond);

            Measure.Method(() => _resolvedFoo = container.Resolve<Foo>())
                .SampleGroup(sampleGroup)
                .WarmupCount(5)
                .MeasurementCount(20)
                .IterationsPerMeasurement(IterationsPerMeasurement)
                .Run();
        }
    }
}
