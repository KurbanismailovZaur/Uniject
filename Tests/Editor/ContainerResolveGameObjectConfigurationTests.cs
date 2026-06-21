using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveGameObjectConfigurationTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WithGameObjectName_RenamesGameObject()
        {
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Player")
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Player"));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_UnderTransform_SetsParent()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WithGameObjectNameAndUnderTransform_AppliesBoth()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Enemy")
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Enemy"));
                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WithGameObjectName_RenamesClonedGameObject()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .WithGameObjectName("Clone")
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Clone"));
                Assert.That(prefabScript.gameObject.name, Is.EqualTo("Prefab"));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WhenContainerParentTransformIsSet_ParentsClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefabScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_UnderTransform_ParentsClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewPrefab(prefab)
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefab.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_AsCached_WithGameObjectNameAndUnderTransform_ConfiguresInstanceOnlyOnce()
        {
            var parent = new GameObject("Parent").transform;
            var first = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Cached")
                    .UnderTransform(parent)
                    .AsCached();

                first = container.Resolve<Script>();
                var second = container.Resolve<Script>();

                Assert.That(second, Is.SameAs(first));
                Assert.That(first.gameObject.name, Is.EqualTo("Cached"));
                Assert.That(first.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (first != null)
                    UnityEngine.Object.DestroyImmediate(first.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WhenContainerParentTransformIsSet_SetsParent()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_WhenContainerParentTransformIsSet_ParentsClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewPrefab(prefab)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefab.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_WhenBindingUnderTransformIsSet_UsesBindingParentInsteadOfContainerParent()
        {
            var containerParent = new GameObject("ContainerParent").transform;
            var bindingParent = new GameObject("BindingParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = containerParent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .UnderTransform(bindingParent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(bindingParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(bindingParent.gameObject);
                UnityEngine.Object.DestroyImmediate(containerParent.gameObject);
            }
        }
    }
}
