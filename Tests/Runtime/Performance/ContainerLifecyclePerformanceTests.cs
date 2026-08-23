using System;
using System.Collections;
using NUnit.Framework;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

namespace Uniject.Tests
{
    public class ContainerLifecyclePerformanceTests
    {
        private const int WarmupCount = 3;
        private const int MeasurementCount = 20;
        private const int FrameWarmupCount = 5;
        private const int FrameMeasurementCount = 30;

        private static readonly int[] TickableCounts = { 0, 100, 1_000, 10_000 };

        private static PerformanceOwnedChildResource _disposeSink;
        private static int _tickSink;

        [TestCase(1)]
        [TestCase(64)]
        [TestCase(256)]
        [TestCase(1024)]
        [Performance, Version("1")]
        public void Dispose_WithOwnedChildContainers_MeasuresScaling(int childContainerCount)
        {
            Container parent = null;

            try
            {
                Measure.Method(() => parent.Dispose())
                    .SetUp(() => parent = CreateParentWithOwnedChildren(childContainerCount))
                    .CleanUp(() =>
                    {
                        parent?.Dispose();
                        parent = null;
                        _disposeSink = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Container.Dispose.{childContainerCount}OwnedChildContainers.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                parent?.Dispose();
                _disposeSink = null;
            }
        }

        [UnityTest, Performance, Version("1")]
        public IEnumerator TickableManager_Update_WithRegisteredTickables_MeasuresRealFrameScaling()
        {
            foreach (var tickableCount in TickableCounts)
            {
                var managerObject = new GameObject($"PerformanceTickableManager{tickableCount}");
                var manager = managerObject.AddComponent<TickableManager>();
                var tickables = new PerformanceTickable[tickableCount];

                for (var i = 0; i < tickableCount; i++)
                {
                    var tickable = new PerformanceTickable();
                    tickables[i] = tickable;
                    manager.RegisterTickable(tickable);
                }

                try
                {
                    yield return Measure.Frames()
                        .SampleGroup(new SampleGroup(
                            $"PlayerLoop.FrameTime.WithTickableManager.{tickableCount}Tickables",
                            SampleUnit.Millisecond))
                        .WarmupCount(FrameWarmupCount)
                        .MeasurementCount(FrameMeasurementCount)
                        .Run();

                    _tickSink = tickableCount == 0
                        ? 0
                        : tickables[tickableCount - 1].Ticks;

                    if (tickableCount > 0)
                        Assert.That(_tickSink, Is.GreaterThan(0));
                }
                finally
                {
                    if (managerObject != null)
                        UnityEngine.Object.DestroyImmediate(managerObject);
                }
            }
        }

        private static Container CreateParentWithOwnedChildren(int childContainerCount)
        {
            var parent = new Container();

            try
            {
                parent.Bind<PerformanceOwnedChildResource>()
                    .FromSubcontainerResolve()
                    .ByMethod(child =>
                        child.Bind<PerformanceOwnedChildResource>()
                            .FromConstructor()
                            .AsCached()
                            .DisposeWithContainer())
                    .AsTransient();
                parent.Build();

                for (var i = 0; i < childContainerCount; i++)
                    _disposeSink = parent.Resolve<PerformanceOwnedChildResource>();

                return parent;
            }
            catch
            {
                parent.Dispose();
                throw;
            }
        }

        [Preserve]
        private sealed class PerformanceOwnedChildResource : IDisposable
        {
            public int DisposeCount { get; private set; }

            [Preserve]
            public PerformanceOwnedChildResource()
            {
            }

            [Preserve]
            public void Dispose()
            {
                DisposeCount++;
            }
        }

        [Preserve]
        private sealed class PerformanceTickable : ITickable
        {
            public int Ticks { get; private set; }

            [Preserve]
            public PerformanceTickable()
            {
            }

            [Preserve]
            public void Tick()
            {
                Ticks++;
            }
        }
    }
}
