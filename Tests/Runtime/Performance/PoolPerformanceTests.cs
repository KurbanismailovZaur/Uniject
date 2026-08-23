using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class PoolPerformanceTests
    {
        private const int ManagedIterationsPerMeasurement = 10_000;
        private const int ComponentIterationsPerMeasurement = 1_000;
        private const int WarmupCount = 5;
        private const int MeasurementCount = 20;

        private static PerformanceManagedPooledObject _managedSink;
        private static PerformancePooledComponent _componentSink;

        [Test, Performance, Version("1")]
        public void Pool_SpawnDespawn_ReusedInstances_MeasuresManagedAndComponent()
        {
            Container container = null;
            PerformanceComponentPool componentPool = null;
            PerformanceComponentWithoutActivationPool componentWithoutActivationPool = null;
            PerformancePooledComponent pooledComponentWithActivation = null;
            PerformancePooledComponent pooledComponentWithoutActivation = null;

            try
            {
                container = new Container();
                container.BindPool<PerformanceManagedPooledObject, PerformanceManagedPool>()
                    .WithInitialSize(1)
                    .WithMaxSize(1)
                    .ExpandByOne()
                    .FromConstructor()
                    .AsCached();
                container.BindPool<PerformancePooledComponent, PerformanceComponentPool>()
                    .WithInitialSize(1)
                    .WithMaxSize(1)
                    .ExpandByOne()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();
                container.BindPool<PerformancePooledComponent, PerformanceComponentWithoutActivationPool>()
                    .WithInitialSize(1)
                    .WithMaxSize(1)
                    .ExpandByOne()
                    .WithoutGameObjectActivation()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();

                var managedPool = container.Resolve<PerformanceManagedPool>();
                componentPool = container.Resolve<PerformanceComponentPool>();
                pooledComponentWithActivation = componentPool.Spawn();
                componentPool.Despawn(pooledComponentWithActivation);

                componentWithoutActivationPool =
                    container.Resolve<PerformanceComponentWithoutActivationPool>();
                pooledComponentWithoutActivation = componentWithoutActivationPool.Spawn();
                componentWithoutActivationPool.Despawn(pooledComponentWithoutActivation);

                Measure.Method(() =>
                    {
                        var instance = managedPool.Spawn();
                        managedPool.Despawn(instance);
                        _managedSink = instance;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Pool.SpawnDespawn.Reused.Managed.{ManagedIterationsPerMeasurement}Cycles",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(ManagedIterationsPerMeasurement)
                    .GC()
                    .Run();

                Measure.Method(() =>
                    {
                        var instance = componentPool.Spawn();
                        componentPool.Despawn(instance);
                        _componentSink = instance;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Pool.SpawnDespawn.Reused.ComponentWithActivation." +
                        $"{ComponentIterationsPerMeasurement}Cycles",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(ComponentIterationsPerMeasurement)
                    .GC()
                    .Run();

                Measure.Method(() =>
                    {
                        var instance = componentWithoutActivationPool.Spawn();
                        componentWithoutActivationPool.Despawn(instance);
                        _componentSink = instance;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Pool.SpawnDespawn.Reused.ComponentWithoutActivation." +
                        $"{ComponentIterationsPerMeasurement}Cycles",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(ComponentIterationsPerMeasurement)
                    .GC()
                    .Run();
            }
            finally
            {
                _managedSink = null;
                _componentSink = null;

                if (pooledComponentWithActivation != null)
                    UnityEngine.Object.DestroyImmediate(pooledComponentWithActivation.gameObject);
                else
                    componentPool?.Clear();

                if (pooledComponentWithoutActivation != null)
                    UnityEngine.Object.DestroyImmediate(pooledComponentWithoutActivation.gameObject);
                else
                    componentWithoutActivationPool?.Clear();

                container?.Dispose();
            }
        }

        [Preserve]
        private sealed class PerformanceManagedPooledObject
        {
            [Preserve]
            public PerformanceManagedPooledObject()
            {
            }
        }

        [Preserve]
        private sealed class PerformanceManagedPool : Pool<PerformanceManagedPooledObject>
        {
            [Preserve]
            public PerformanceManagedPool()
            {
            }
        }

        [Preserve]
        private sealed class PerformanceComponentPool : Pool<PerformancePooledComponent>
        {
            [Preserve]
            public PerformanceComponentPool()
            {
            }
        }

        [Preserve]
        private sealed class PerformanceComponentWithoutActivationPool : Pool<PerformancePooledComponent>
        {
            [Preserve]
            public PerformanceComponentWithoutActivationPool()
            {
            }
        }
    }
}
