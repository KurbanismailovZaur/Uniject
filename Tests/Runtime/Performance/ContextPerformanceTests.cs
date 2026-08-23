using NUnit.Framework;
using Uniject.Contexts;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ContextPerformanceTests
    {
        private const int WarmupCount = 3;
        private const int MeasurementCount = 20;
        private const int ResolveIterationsPerMeasurement = 100;

        private static PerformanceHierarchyTarget _hierarchySink;

        [TestCase(1)]
        [TestCase(64)]
        [TestCase(256)]
        [TestCase(1024)]
        [Performance, Version("1")]
        public void GameObjectContext_Run_AutoInjection_MeasuresScaling(int targetCount)
        {
            GameObject contextObject = null;
            GameObjectContext context = null;

            try
            {
                Measure.Method(() => context.Run())
                    .SetUp(() =>
                    {
                        contextObject = CreateAutoInjectionContext(targetCount);
                        context = contextObject.GetComponent<GameObjectContext>();
                    })
                    .CleanUp(() =>
                    {
                        context?.Container?.Dispose();

                        if (contextObject != null)
                            UnityEngine.Object.DestroyImmediate(contextObject);

                        contextObject = null;
                        context = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"GameObjectContext.Run.AutoInjection.{targetCount}Targets.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                context?.Container?.Dispose();

                if (contextObject != null)
                    UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [TestCase(1)]
        [TestCase(64)]
        [TestCase(256)]
        [TestCase(1024)]
        [Performance, Version("1")]
        public void ResolveTransient_FromComponentInHierarchy_MeasuresSearchScaling(int searchedChildCount)
        {
            var contextObject = CreateHierarchyContext(searchedChildCount, out var context);

            try
            {
                Measure.Method(() => _hierarchySink = context.Container.Resolve<PerformanceHierarchyTarget>())
                    .SampleGroup(new SampleGroup(
                        $"Container.ResolveTransient.FromComponentInHierarchy." +
                        $"{searchedChildCount}Children.{ResolveIterationsPerMeasurement}Calls",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(ResolveIterationsPerMeasurement)
                    .GC()
                    .Run();
            }
            finally
            {
                _hierarchySink = null;
                context?.Container?.Dispose();

                if (contextObject != null)
                    UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        private static GameObject CreateAutoInjectionContext(int targetCount)
        {
            var contextObject = new GameObject("PerformanceAutoInjectionContext");

            try
            {
                contextObject.SetActive(false);
                contextObject.AddComponent<GameObjectContext>();
                contextObject.AddComponent<PerformanceContextInstaller>();

                for (var i = 0; i < targetCount; i++)
                {
                    var targetObject = new GameObject($"PerformanceInjectTarget{i}");
                    targetObject.transform.SetParent(contextObject.transform, false);
                    targetObject.AddComponent<PerformanceContextInjectTarget>();
                }

                return contextObject;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(contextObject);
                throw;
            }
        }

        private static GameObject CreateHierarchyContext(
            int searchedChildCount,
            out GameObjectContext context)
        {
            var contextObject = new GameObject("PerformanceHierarchyContext");
            context = null;

            try
            {
                contextObject.SetActive(false);
                context = contextObject.AddComponent<GameObjectContext>();

                for (var i = 0; i < searchedChildCount; i++)
                {
                    var child = new GameObject($"PerformanceHierarchyNode{i}");
                    child.transform.SetParent(contextObject.transform, false);

                    if (i == searchedChildCount - 1)
                        child.AddComponent<PerformanceHierarchyTarget>();
                }

                context.Initialize();
                context.Container.Bind<PerformanceHierarchyTarget>()
                    .FromComponentInHierarchy()
                    .AsTransient();
                context.Container.Build();
                return contextObject;
            }
            catch
            {
                context?.Container?.Dispose();
                UnityEngine.Object.DestroyImmediate(contextObject);
                throw;
            }
        }

        [Preserve]
        public sealed class PerformanceContextDependency
        {
            [Preserve]
            public PerformanceContextDependency()
            {
            }
        }
    }
}
