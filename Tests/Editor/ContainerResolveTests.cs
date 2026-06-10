using System;
using NUnit.Framework;
using Uniject;
using Uniject.Exceptions;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveTests
    {
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
                container.Bind<Class>().To<Class>();
                container.Bind<IInterface>().To<ClassImplementedIInterface>();
            }, Throws.Nothing);
        }

        [Test]
        public void Resolve_WhenSameConcreteTypeIsBoundToDifferentContracts_ReturnsInstances()
        {
            var container = new Container();

            container.Bind<ClassImplementedIInterface>().To<ClassImplementedIInterface>();
            container.Bind<IInterface>().To<ClassImplementedIInterface>();

            var concreteInstance = container.Resolve<ClassImplementedIInterface>();
            var interfaceInstance = container.Resolve<IInterface>();

            Assert.That(concreteInstance, Is.Not.Null);
            Assert.That(interfaceInstance, Is.Not.Null);
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
        public void Bind_WhenContractTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_WhenTypeAlreadyBoundUsingNonGenericBind_ThrowsBindingException()
        {
            var container = new Container();
            container.Bind(typeof(Class));

            Assert.That(
                () => container.Bind<Class>(),
                Throws.TypeOf<BindingException>());
        }

        [Test]
        public void Bind_WhenTypeAlreadyBoundUsingGenericBind_ThrowsBindingException()
        {
            var container = new Container();
            container.Bind<Class>();

            Assert.That(
                () => container.Bind(typeof(Class)),
                Throws.TypeOf<BindingException>());
        }

        [Test]
        public void Resolve_WhenTypeWasBoundUsingNonGenericBind_ReturnsInstance()
        {
            var container = new Container();
            container.Bind(typeof(Class));

            var instance = container.Resolve<Class>();

            Assert.That(instance, Is.TypeOf<Class>());
        }

        [Test]
        public void Resolve_WhenNonGenericBindUsesNonGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind(typeof(IInterface)).To(typeof(ClassImplementedIInterface));

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Resolve_WhenNonGenericBindUsesGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind(typeof(IInterface)).To<ClassImplementedIInterface>();

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Resolve_WhenGenericBindUsesNonGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind<IInterface>().To(typeof(ClassImplementedIInterface));

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Bind_To_WhenConcreteTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(typeof(IInterface)).To(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_To_WhenConcreteTypeIsNotAssignableToContract_ThrowsBindingException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(typeof(IInterface)).To(typeof(Class)),
                Throws.TypeOf<BindingException>());
        }

        [Test]
        public void Bind_To_WhenGenericBindUsesNotAssignableConcreteType_ThrowsBindingException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().To(typeof(Class)),
                Throws.TypeOf<BindingException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<IInterface>().To<IInterface>().FromConstructor();

            Assert.That(
                () => container.Resolve<IInterface>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<AbstractClass>().To<AbstractClass>().FromConstructor();

            Assert.That(
                () => container.Resolve<AbstractClass>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<Script>().To<Script>().FromConstructor();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenNoPublicConstructor_ThrowsException()
        {
            var container = new Container();
            container.Bind<ClassWithPrivateConstructor>().To<ClassWithPrivateConstructor>().FromConstructor();

            Assert.That(
                () => container.Resolve<ClassWithPrivateConstructor>(),
                Throws.Exception);
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
        public void Resolve_FromInstance_WhenContractIsInterface_ReturnsSameInstance()
        {
            IInterface instance = new ClassImplementedIInterface();

            var container = new Container();
            container.Bind<IInterface>().FromInstance(instance);

            var resolvedInstance = container.Resolve<IInterface>();

            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Resolve_FromInstance_WhenContractIsAbstractClass_ReturnsSameInstance()
        {
            AbstractClass instance = new ClassImplementedAbstractClass();

            var container = new Container();
            container.Bind<AbstractClass>().FromInstance(instance);

            var resolvedInstance = container.Resolve<AbstractClass>();

            Assert.That(resolvedInstance, Is.SameAs(instance));
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
        public void Resolve_FromInstance_WhenBoundUsingNonGenericBind_ReturnsSameInstance()
        {
            IInterface instance = new ClassImplementedIInterface();

            var container = new Container();
            container.Bind(typeof(IInterface)).FromInstance(instance);

            var resolvedInstance = container.Resolve<IInterface>();

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
                () => container.Bind<Script>().To<Script>().FromComponentInNewPrefab((Component)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenConcreteTypeIsNotComponentOrInterface_ThrowsArgumentException()
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
        public void Bind_FromComponentInNewPrefab_WhenPrefabDoesNotHaveRequestedComponent_ThrowsArgumentException()
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
        public void Resolve_FromComponentInNewPrefab_WhenConcreteTypeIsInterface_ReturnsComponentImplementingInterface()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var resolved = default(IInterface);

            try
            {
                var container = new Container();
                container.Bind<IInterface>().To<IInterface>().FromComponentInNewPrefab(prefabScript.gameObject);

                resolved = container.Resolve<IInterface>();

                Assert.That(resolved, Is.Not.Null);
                Assert.That(resolved, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(resolved, Is.Not.SameAs(prefabScript));
                Assert.That(((ScriptImplementedIInterface)resolved).gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (resolved != null)
                    UnityEngine.Object.DestroyImmediate(((ScriptImplementedIInterface)resolved).gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Bind_FromComponentInNewPrefab_WhenPrefabDoesNotHaveComponentImplementingInterface_ThrowsArgumentException()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<IInterface>().To<IInterface>().FromComponentInNewPrefab(prefabScript.gameObject),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
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

        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().To<IInterface>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<AbstractScript>().To<AbstractScript>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_ReturnsComponentOnNewGameObject()
        {
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromNewComponentOnNewGameObject();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript.gameObject, Is.Not.Null);
                Assert.That(resolvedScript.gameObject.name, Is.EqualTo(nameof(Script)));
                Assert.That(resolvedScript.gameObject.GetComponent<Script>(), Is.SameAs(resolvedScript));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_InjectsAddedComponent()
        {
            var dependency = new Class();
            var resolvedScript = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().To<Class>().FromInstance(dependency);
                container.Bind<InjectableScript>().To<InjectableScript>().FromNewComponentOnNewGameObject();

                resolvedScript = container.Resolve<InjectableScript>();

                Assert.That(resolvedScript.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WithObjectName_RenamesGameObject()
        {
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithObjectName("Player")
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
        public void Resolve_FromNewComponentOnNewGameObject_WithObjectNameAndUnderTransform_AppliesBoth()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithObjectName("Enemy")
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
        public void Resolve_FromComponentInNewPrefab_WithObjectName_RenamesClonedGameObject()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .WithObjectName("Clone")
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
        public void Resolve_AsCached_WithObjectNameAndUnderTransform_ConfiguresInstanceOnlyOnce()
        {
            var parent = new GameObject("Parent").transform;
            var first = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithObjectName("Cached")
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
        public void Bind_FromResolve_WhenContractTypeEqualsConcreteType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromResolve(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteTypeIsNotBound_ThrowsException()
        {
            var container = new Container();
            container.Bind<IInterface>().To<ClassImplementedIInterface>().FromResolve();

            Assert.That(
                () => container.Resolve<IInterface>(),
                Throws.Exception);
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteBindingIsTransient_ReturnsDifferentInstances()
        {
            var container = new Container();
            container.Bind<ClassImplementedIInterface>();
            container.Bind<IInterface>().To<ClassImplementedIInterface>().FromResolve();

            var first = container.Resolve<IInterface>();
            var second = container.Resolve<IInterface>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_AsTransient_ReturnsDifferentInstances()
        {
            var container = new Container();
            container.Bind<Class>().AsTransient();

            var first = container.Resolve<Class>();
            var second = container.Resolve<Class>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_AsCached_ReturnsSameInstance()
        {
            var container = new Container();
            container.Bind<Class>().AsCached();

            var first = container.Resolve<Class>();
            var second = container.Resolve<Class>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }
    }
}
