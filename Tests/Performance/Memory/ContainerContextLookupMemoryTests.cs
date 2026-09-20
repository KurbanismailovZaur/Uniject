using NUnit.Framework;
using Uniject.Contexts;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerContextLookupMemoryTests
    {
        [Test, Performance]
        public void GetNearestContext([Values(0, 1, 4, 16)] int depth, [Values] bool withContext)
        {
            var gameObject = withContext ? new GameObject("MemoryContext") : null;
            Container[] hierarchy = null;
            try
            {
                var context = withContext ? gameObject.AddComponent<GameObjectContext>() : null;
                context?.Initialize();
                hierarchy = Hierarchy(withContext ? context.Container : new Container(), depth);
                var leaf = hierarchy[depth];
                Context result = null;
                AllocationMeasurement.Run("Context.Nearest", () => result = leaf.GetNearestContext(), expectZero: true);
                Assert.That(result, Is.SameAs(context));
            }
            finally
            {
                if (hierarchy != null) DisposeHierarchy(hierarchy);
                if (gameObject != null) Object.DestroyImmediate(gameObject);
            }
        }

        [Test, Performance]
        public void GetInfoAboutNearestParent([Values(0, 1, 4, 16)] int depth, [Values] ParentKind kind)
        {
            var gameObject = new GameObject("MemoryParent");
            var contextObject = new GameObject("MemoryBoundary");
            Container[] hierarchy = null;
            Container ancestor = null;
            try
            {
                Container root;
                Context expectedContext = null;
                Transform expectedParent = null;
                switch (kind)
                {
                    case ParentKind.Transform:
                        expectedParent = gameObject.transform;
                        root = new Container(parentTransformForGameObjects: expectedParent);
                        break;
                    case ParentKind.GameObjectContext:
                        expectedContext = contextObject.AddComponent<GameObjectContext>();
                        expectedContext.Initialize();
                        root = expectedContext.Container;
                        expectedParent = contextObject.transform;
                        break;
                    case ParentKind.SceneBoundary:
                        ancestor = new Container(parentTransformForGameObjects: gameObject.transform);
                        expectedContext = contextObject.AddComponent<SceneContext>();
                        expectedContext.Initialize(ancestor);
                        root = expectedContext.Container;
                        break;
                    default:
                        root = new Container();
                        break;
                }
                hierarchy = Hierarchy(root, depth);
                var leaf = hierarchy[depth];
                (Context context, Transform parentTransform) result = default;
                AllocationMeasurement.Run("Context.ParentInfo", () => result = leaf.GetInfoAboutNearestParentForGameObjects(), expectZero: true);
                Assert.That(result.context, Is.SameAs(expectedContext));
                Assert.That(result.parentTransform, Is.SameAs(expectedParent));
            }
            finally
            {
                if (hierarchy != null) DisposeHierarchy(hierarchy);
                ancestor?.Dispose();
                Object.DestroyImmediate(contextObject);
                Object.DestroyImmediate(gameObject);
            }
        }

        public enum ParentKind { None, Transform, GameObjectContext, SceneBoundary }
    }
}
