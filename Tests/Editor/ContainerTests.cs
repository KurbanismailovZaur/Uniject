using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Exceptions;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerTests
    {
        // Тестить надо публичное API
        // МетодИлиФича_УсловиеТеста_ОжидаемыйРезультат

        // container.Bind<Class>().To<Class>().FromConstructor().AsTransient().NonLazy();        

        // Duplicate bindings
        // From
        //   Check exceptions
        // As
        // NonLazy
        // From/As/NonLazy shortcuts
        // Circular dependencies
        // Inject
        // Instantiate

        [Test]
        public void Bind_WhenTypeAlreadyBound_ThrowsBindingException()
        {
            var container = new Container();
            container.Bind<Class>();

            Assert.That(
                () => container.Bind<Class>(),
                Throws.TypeOf<BindingException>());
        }

        [Test]
        public void Bind_WhenDifferentTypesAreBound_DoesNotThrow()
        {
            var container = new Container();

            Assert.That(() =>
            {
                container.Bind<Class>();
                container.Bind<IInterface>().To<ClassImplementedIInterface>();
            }, Throws.Nothing);
        }

        [Test]
        public void Bind_WhenSameConcreteTypeIsBoundToDifferentContracts_DoesNotThrow()
        {
            var container = new Container();

            container.Bind<Class>();
            container.Bind<IInterface>().To<ClassImplementedIInterface>();

            Assert.Pass();
        }

        [Test]
        public void Resolve_WhenTypeWasBound_ReturnsInstance()
        {
            var container = new Container();
            container.Bind<Class>();

            var instance = container.Resolve<Class>();
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void Resolve_FromConstructor_ReturnsNewInstance()
        {
            var container = new Container();
            container.Bind<Class>().To<Class>().FromConstructor();

            var instance = container.Resolve<Class>();
            Assert.IsNotNull(instance);
        }

        [Test]
        public void Bind_FromInstance_WhenInstanceIsNull_ThrowsArgumentException()
        {
            var container = new Container();
            Assert.That(
                () => container.Bind<Class>().To<Class>().FromInstance(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_FromInstance_ReturnsSameInstance()
        {
            var instance = new Class();

            var container = new Container();
            container.Bind<Class>().To<Class>().FromInstance(instance);

            var resolvedInstance = container.Resolve<Class>();
            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenGameObjectPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Script>().To<Script>().FromComponentInNewPrefab((GameObject)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenComponentPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Script>().To<Script>().FromComponentInNewPrefab((Script)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var prefab = new GameObject("Prefab");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<Class>().To<Class>().FromComponentInNewPrefab(prefab),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenGameObjectDoesNotHaveComponent_ThrowsArgumentException()
        {
            var prefab = new GameObject("Prefab");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<Script>().To<Script>().FromComponentInNewPrefab(prefab),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenPrefabIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromComponentInNewPrefab(new Class()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WhenPrefabIsGameObject_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromComponentInNewPrefab(prefabScript.gameObject);

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript, Is.Not.SameAs(prefabScript));
                Assert.That(resolvedScript.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WhenPrefabIsComponent_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromComponentInNewPrefab(prefabScript);

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript, Is.Not.SameAs(prefabScript));
                Assert.That(resolvedScript.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
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
                () => container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab((Script)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewPrefab_WhenPrefabIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromNewComponentOnNewPrefab(new Class()),
                Throws.TypeOf<ArgumentException>());
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
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromNewComponentOnNewPrefab(prefabScript);

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript, Is.Not.SameAs(prefabScript));
                Assert.That(resolvedScript.gameObject, Is.Not.SameAs(prefabScript.gameObject));
                Assert.That(prefabScript.gameObject.GetComponents<Script>(), Has.Length.EqualTo(1));
                Assert.That(resolvedScript.gameObject.GetComponents<Script>(), Has.Length.EqualTo(2));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }
    }
}
