using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Uniject.Contexts;
using UnityEngine;

namespace Uniject.Tests.Memory
{
    public sealed class MemoryContextPayloadComponent : MonoBehaviour
    {
        [NonSerialized] public object Payload;
    }

    [Category("MemoryRetention")]
    public class ContextRetentionTests
    {
        public enum ContextKind { GameObject, Scene }

        private sealed class DisposablePayload : IDisposable
        {
            public void Dispose() { }
        }

        [Test]
        public void DestroyContext_ReleasesContainerPayloadsWhileWrappersRemainAlive(
            [Values] ContextKind kind, [Values] bool build)
        {
            var gameObject = new GameObject(nameof(ContextRetentionTests));
            try
            {
                var context = CreateContext(gameObject, kind);
                ContextTestUtility.Configure(context);
                context.Initialize();
                var references = Populate(context.Container);
                if (build) context.Build();

                UnityEngine.Object.DestroyImmediate(gameObject);

                Assert.That(context == null, Is.True, "The native component must have been destroyed.");
                MemoryRetentionAssert.Collected(new object[] { context, context.Container }, references);
            }
            finally
            {
                if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void DestroyNestedContexts_ReleasesBothContainerPayloads()
        {
            var gameObject = new GameObject("Parent context");
            try
            {
                var parent = (GameObjectContext)CreateContext(gameObject, ContextKind.GameObject);
                var childObject = new GameObject("Child context");
                childObject.transform.SetParent(gameObject.transform);
                var child = (GameObjectContext)CreateContext(childObject, ContextKind.GameObject);
                ContextTestUtility.Configure(child);
                ContextTestUtility.Configure(parent, gameObjectContexts: new[] { child });
                parent.Initialize();
                var parentReferences = Populate(parent.Container);
                var childReferences = Populate(child.Container);
                parent.Build();

                UnityEngine.Object.DestroyImmediate(gameObject);

                var owners = new object[] { parent, child, parent.Container, child.Container };
                MemoryRetentionAssert.Collected(owners, parentReferences);
                MemoryRetentionAssert.Collected(owners, childReferences);
            }
            finally
            {
                if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void HierarchyScan_DoesNotRetainDestroyedTargetPayloadsInStaticCollections()
        {
            var reference = ScanAndDestroy(false, out var owners);

            MemoryRetentionAssert.Collected(owners, reference);
        }

        [Test]
        public void RepeatedHierarchyScanAndDestroy_DoesNotRetainPreviousPayloads()
        {
            var owners = new object[8];
            var references = new WeakReference[owners.Length];
            for (var i = 0; i < owners.Length; i++)
                references[i] = ScanAndDestroy(false, out owners[i]);

            MemoryRetentionAssert.Collected(owners, references);
        }

        [Test]
        [Explicit("Known retention regression: hierarchy scan does not return rented collections when queueing throws.")]
        public void HierarchyScan_WhenQueueingThrows_DoesNotRetainDestroyedTargetPayload()
        {
            // Isolate the global pool so an intentionally failing regression cannot poison other tests.
            var field = GetStaticPoolField();
            var original = (CollectionPool)field.GetValue(null);
            using var isolatedPool = new CollectionPool();
            field.SetValue(null, isolatedPool);
            try
            {
                var reference = ScanAndDestroy(true, out var owners);

                MemoryRetentionAssert.Collected(new object[] { owners, isolatedPool }, reference);
            }
            finally
            {
                field.SetValue(null, original);
            }
        }

        private static Context CreateContext(GameObject gameObject, ContextKind kind)
        {
            Context context = kind == ContextKind.GameObject
                ? gameObject.AddComponent<GameObjectContext>()
                : gameObject.AddComponent<SceneContext>();
            // Matches ContextDisposalTests: invoke the real OnDestroy lifecycle in EditMode.
            context.runInEditMode = true;
            return context;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] Populate(Container container)
        {
            var cached = new DisposablePayload();
            var queued = new object();
            container.BindInstance(cached).DisposeWithContainer();
            container.AddToInjectionQueue(queued);
            return new[] { new WeakReference(cached), new WeakReference(queued) };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ScanAndDestroy(bool failQueueing, out object owners)
        {
            var gameObject = new GameObject("Memory hierarchy context");
            try
            {
                var context = (GameObjectContext)CreateContext(gameObject, ContextKind.GameObject);
                var child = new GameObject("Injection target");
                child.transform.SetParent(gameObject.transform);
                var target = child.AddComponent<MemoryContextPayloadComponent>();
                target.Payload = new object();
                var reference = new WeakReference(target.Payload);
                ContextTestUtility.Configure(context, injectInAllContextGameObjects: true);
                context.Initialize();
                if (failQueueing)
                {
                    context.Container.AddToInjectionQueue(target);
                    Assert.Throws<ArgumentException>(() => context.Install());
                }
                else
                {
                    context.Install();
                    context.Build();
                }

                owners = new object[] { context, context.Container, GetStaticPoolField().GetValue(null) };
                return reference;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static FieldInfo GetStaticPoolField()
        {
            var type = typeof(Container).Assembly.GetType("Uniject.StaticCollections", true);
            var field = type.GetField("collectionPool", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return field;
        }
    }
}
