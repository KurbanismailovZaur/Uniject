using System;
using System.Reflection;
using NUnit.Framework;
using Uniject.Components;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class UnityObjectPerformanceTests
    {
        private const int WarmupCount = 3;
        private const int MeasurementCount = 20;

        private static GameObject _gameObjectSink;
        private static PerformanceUnityObjectInjectTarget _componentSink;

        [TestCase(1)]
        [TestCase(16)]
        [TestCase(64)]
        [Performance, Version("1")]
        public void InstantiatePrefab_WithInjectTargets_ComparedToRawInstantiate(int injectTargetCount)
        {
            GameObject prefab = null;
            Container container = null;
            GameObject clone = null;

            try
            {
                prefab = CreateInjectablePrefab(injectTargetCount);
                container = CreateContainer();

                Measure.Method(() =>
                    {
                        clone = UnityEngine.Object.Instantiate(prefab);
                        _gameObjectSink = clone;
                    })
                    .SetUp(() => clone = null)
                    .CleanUp(() =>
                    {
                        if (clone != null)
                            UnityEngine.Object.DestroyImmediate(clone);

                        clone = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"UnityEngine.Object.Instantiate.Prefab.{injectTargetCount}InjectTargets.Raw.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();

                Measure.Method(() =>
                    {
                        clone = container.Instantiate(prefab);
                        _gameObjectSink = clone;
                    })
                    .SetUp(() => clone = null)
                    .CleanUp(() =>
                    {
                        if (clone != null)
                            UnityEngine.Object.DestroyImmediate(clone);

                        clone = null;
                    })
                    .SampleGroup(new SampleGroup(
                        $"Container.Instantiate.Prefab.{injectTargetCount}InjectTargets.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                _gameObjectSink = null;

                if (clone != null)
                    UnityEngine.Object.DestroyImmediate(clone);

                if (prefab != null)
                    UnityEngine.Object.DestroyImmediate(prefab);

                container?.Dispose();
            }
        }

        [Test, Performance, Version("1")]
        public void AddComponent_Injectable_ComparedToRawAddComponent()
        {
            var rawHost = new GameObject("PerformanceRawAddComponentHost");
            var injectedHost = new GameObject("PerformanceInjectedAddComponentHost");
            rawHost.SetActive(false);
            injectedHost.SetActive(false);
            Container container = null;
            PerformanceUnityObjectInjectTarget addedComponent = null;

            try
            {
                container = CreateContainer();

                Measure.Method(() =>
                    {
                        addedComponent = rawHost.AddComponent<PerformanceUnityObjectInjectTarget>();
                        _componentSink = addedComponent;
                    })
                    .SetUp(() => addedComponent = null)
                    .CleanUp(() =>
                    {
                        if (addedComponent != null)
                            UnityEngine.Object.DestroyImmediate(addedComponent);

                        addedComponent = null;
                    })
                    .SampleGroup(new SampleGroup(
                        "GameObject.AddComponent.Injectable.Raw.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();

                Measure.Method(() =>
                    {
                        addedComponent =
                            container.AddComponent<PerformanceUnityObjectInjectTarget>(injectedHost);
                        _componentSink = addedComponent;
                    })
                    .SetUp(() => addedComponent = null)
                    .CleanUp(() =>
                    {
                        if (addedComponent != null)
                            UnityEngine.Object.DestroyImmediate(addedComponent);

                        addedComponent = null;
                    })
                    .SampleGroup(new SampleGroup(
                        "Container.AddComponent.Injectable.OneShot",
                        SampleUnit.Microsecond))
                    .WarmupCount(WarmupCount)
                    .MeasurementCount(MeasurementCount)
                    .IterationsPerMeasurement(1)
                    .GC()
                    .Run();
            }
            finally
            {
                _componentSink = null;

                if (addedComponent != null)
                    UnityEngine.Object.DestroyImmediate(addedComponent);

                if (rawHost != null)
                    UnityEngine.Object.DestroyImmediate(rawHost);

                if (injectedHost != null)
                    UnityEngine.Object.DestroyImmediate(injectedHost);

                container?.Dispose();
            }
        }

        private static Container CreateContainer()
        {
            var container = new Container();

            try
            {
                container.BindInstance(new PerformanceUnityObjectDependency());
                container.Build();
                return container;
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }

        private static GameObject CreateInjectablePrefab(int injectTargetCount)
        {
            var prefab = new GameObject("PerformanceInjectablePrefab");

            try
            {
                prefab.SetActive(false);
                var targets = new MonoBehaviour[injectTargetCount];

                for (var i = 0; i < injectTargetCount; i++)
                    targets[i] = prefab.AddComponent<PerformanceUnityObjectInjectTarget>();

                var injectTargets = prefab.AddComponent<InjectTargets>();
                SetInjectTargets(injectTargets, targets);
                return prefab;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(prefab);
                throw;
            }
        }

        private static void SetInjectTargets(InjectTargets injectTargets, MonoBehaviour[] targets)
        {
            var fieldName = $"<{nameof(InjectTargets.Targets)}>k__BackingField";
            var targetsField = typeof(InjectTargets).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (targetsField == null)
                throw new MissingFieldException(typeof(InjectTargets).FullName, fieldName);

            targetsField.SetValue(injectTargets, targets);
        }

        [Preserve]
        public sealed class PerformanceUnityObjectDependency
        {
            [Preserve]
            public PerformanceUnityObjectDependency()
            {
            }
        }
    }
}
