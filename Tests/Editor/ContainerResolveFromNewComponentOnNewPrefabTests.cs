using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromNewComponentOnNewPrefabTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_FromNewComponentOnNewPrefab_WhenGameObjectPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab((GameObject)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewPrefab_WhenComponentPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab((Component)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewPrefab_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var prefab = new GameObject("Prefab");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<Class>().To<Class>().FromNewComponentOnNewPrefab(prefab),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_WhenPrefabIsGameObject_AddsComponentToClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab(prefab);

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript.gameObject, Is.Not.SameAs(prefab));
                Assert.That(resolvedScript.gameObject.GetComponent<Script>(), Is.SameAs(resolvedScript));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_WhenPrefabIsComponent_AddsComponentToClonedPrefab()
        {
            var prefabComponent = new GameObject("Prefab").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab(prefabComponent);

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript, Is.Not.SameAs(prefabComponent));
                Assert.That(resolvedScript.gameObject, Is.Not.SameAs(prefabComponent.gameObject));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabComponent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_InjectsAddedComponent()
        {
            var prefab = new GameObject("Prefab");
            var dependency = new Class();
            var resolvedScript = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().To<Class>().FromInstance(dependency);
                container.Bind<InjectableScript>().To<InjectableScript>().FromNewComponentOnNewPrefab(prefab);

                resolvedScript = container.Resolve<InjectableScript>();

                Assert.That(resolvedScript.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }
    }
}
