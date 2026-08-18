using System;
using NUnit.Framework;
using Uniject.Contexts;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContextDisposalTests
    {
        private sealed class DisposableResource : IDisposable
        {
            public int DisposeCallsCount { get; private set; }

            public void Dispose() => DisposeCallsCount++;
        }

        [Test]
        public void OnDestroy_WhenContextIsInitializedButNotBuilt_DisposesMarkedInstance()
        {
            var contextObject = new GameObject("GameObjectContext");
            var resource = new DisposableResource();

            try
            {
                var context = contextObject.AddComponent<GameObjectContext>();
                context.runInEditMode = true;
                context.Initialize();
                context.Container.BindInstance(resource).DisposeWithContainer();

                Assert.That(context.IsInitialized, Is.True);
                Assert.That(context.IsBuilded, Is.False);

                UnityEngine.Object.DestroyImmediate(contextObject);

                Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
                GC.KeepAlive(context);
            }
            finally
            {
                if (contextObject != null)
                    UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Dispose_ParentWithGeneratedContext_DoesNotDisposeContextUntilGameObjectIsDestroyed()
        {
            var parent = new Container();
            var resource = new DisposableResource();
            GameObjectContext generatedContext = null;
            GameObject generatedContextObject = null;

            try
            {
                parent.Bind<DisposableResource>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(child =>
                    {
                        generatedContext = (GameObjectContext)child.Context;
                        generatedContext.runInEditMode = true;
                        generatedContextObject = generatedContext.gameObject;
                        child.BindInstance(resource).DisposeWithContainer();
                    })
                    .AsCached();

                var resolved = parent.Resolve<DisposableResource>();

                Assert.That(resolved, Is.SameAs(resource));
                Assert.That(generatedContextObject, Is.Not.Null);

                parent.Dispose();

                Assert.That(resource.DisposeCallsCount, Is.Zero);

                UnityEngine.Object.DestroyImmediate(generatedContext.gameObject);

                Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
                GC.KeepAlive(generatedContext);
            }
            finally
            {
                if (generatedContextObject != null)
                    UnityEngine.Object.DestroyImmediate(generatedContextObject);

                parent.Dispose();
            }
        }
    }
}
