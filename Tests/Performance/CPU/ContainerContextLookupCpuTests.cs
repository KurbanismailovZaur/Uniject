using NUnit.Framework;
using Uniject.Contexts;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerContextLookupCpuTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetNearestContext_WithContext(int depth)
        {
            var gameObject = new GameObject("Context");
            var context = gameObject.AddComponent<GameObjectContext>();
            context.Initialize();
            var containers = CreateHierarchy(context.Container, depth);
            var leaf = containers[depth];
            leaf.GetNearestContext();

            Measure.Method(() => leaf.GetNearestContext())
                .SampleGroup(new SampleGroup("GetNearestContext", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetNearestContext_WithoutContext(int depth)
        {
            using var root = new Container();
            var containers = CreateHierarchy(root, depth);
            var leaf = containers[depth];
            leaf.GetNearestContext();

            Measure.Method(() => leaf.GetNearestContext())
                .SampleGroup(new SampleGroup("GetNearestContext", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetInfoAboutNearestParentForGameObjects_WithParentTransform(int depth)
        {
            var gameObject = new GameObject("Parent");
            using var root = new Container(parentTransformForGameObjects: gameObject.transform);
            var containers = CreateHierarchy(root, depth);
            var leaf = containers[depth];
            leaf.GetInfoAboutNearestParentForGameObjects();

            Measure.Method(() => leaf.GetInfoAboutNearestParentForGameObjects())
                .SampleGroup(new SampleGroup("GetInfoAboutNearestParentForGameObjects", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetInfoAboutNearestParentForGameObjects_GameObjectContext(int depth)
        {
            var gameObject = new GameObject("Context");
            var context = gameObject.AddComponent<GameObjectContext>();
            context.Initialize();
            var containers = CreateHierarchy(context.Container, depth);
            var leaf = containers[depth];
            leaf.GetInfoAboutNearestParentForGameObjects();

            Measure.Method(() => leaf.GetInfoAboutNearestParentForGameObjects())
                .SampleGroup(new SampleGroup("GetInfoAboutNearestParentForGameObjects", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
            Object.DestroyImmediate(gameObject);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetInfoAboutNearestParentForGameObjects_SceneContextBoundary(int depth)
        {
            var parent = new GameObject("Parent");
            using var ancestor = new Container(parentTransformForGameObjects: parent.transform);
            var gameObject = new GameObject("SceneContext");
            var context = gameObject.AddComponent<SceneContext>();
            context.Initialize(ancestor);
            var containers = CreateHierarchy(context.Container, depth);
            var leaf = containers[depth];
            leaf.GetInfoAboutNearestParentForGameObjects();

            Measure.Method(() => leaf.GetInfoAboutNearestParentForGameObjects())
                .SampleGroup(new SampleGroup("GetInfoAboutNearestParentForGameObjects", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(parent);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void GetInfoAboutNearestParentForGameObjects_WithoutParent(int depth)
        {
            using var root = new Container();
            var containers = CreateHierarchy(root, depth);
            var leaf = containers[depth];
            leaf.GetInfoAboutNearestParentForGameObjects();

            Measure.Method(() => leaf.GetInfoAboutNearestParentForGameObjects())
                .SampleGroup(new SampleGroup("GetInfoAboutNearestParentForGameObjects", SampleUnit.Nanosecond))
                .Run();

            DisposeDescendants(containers);
        }

        private static Container[] CreateHierarchy(Container root, int depth)
        {
            var containers = new Container[depth + 1];
            containers[0] = root;
            for (var i = 1; i < containers.Length; i++)
                containers[i] = new Container(containers[i - 1]);

            return containers;
        }

        private static void DisposeDescendants(Container[] containers)
        {
            for (var i = containers.Length - 1; i > 0; i--)
                containers[i].Dispose();
        }
    }
}
