using System;
using System.Reflection;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Exceptions;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerTests
    {
        private class NonLazyTransientClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyTransientClass() => InstancesCount++;
        }

        private class NonLazyCachedClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyCachedClass() => InstancesCount++;
        }

        private class NonLazyShortcutClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyShortcutClass() => InstancesCount++;
        }

        private class ConstructorCircularA
        {
            public ConstructorCircularA(ConstructorCircularB dependency) { }
        }

        private class ConstructorCircularB
        {
            public ConstructorCircularB(ConstructorCircularA dependency) { }
        }

        private interface IFromResolveCircularDependency
        {
        }

        private class FromResolveCircularDependency : IFromResolveCircularDependency
        {
            public FromResolveCircularDependency(IFromResolveCircularDependency dependency) { }
        }

        private class InjectableClass
        {
            public Class Dependency { get; private set; }
            public int CallsCount { get; private set; }

            [Inject]
            public void Construct(Class dependency)
            {
                Dependency = dependency;
                CallsCount++;
            }
        }

        private class ParameterlessInjectableClass
        {
            public bool WasInjected { get; private set; }

            [Inject]
            public void Construct() => WasInjected = true;
        }

        private class MultiDependencyInjectableClass
        {
            public Class ClassDependency { get; private set; }
            public IInterface InterfaceDependency { get; private set; }

            [Inject]
            public void Construct(Class classDependency, IInterface interfaceDependency)
            {
                ClassDependency = classDependency;
                InterfaceDependency = interfaceDependency;
            }
        }

        private class ClassWithoutInjectMethod
        {
            public bool WasConstructCalled { get; private set; }

            public void Construct(Class dependency) => WasConstructCalled = true;
        }

        private class ClassWithMultipleInjectMethods
        {
            [Inject]
            public void Construct(Class dependency) { }

            [Inject]
            public void Initialize(Class dependency) { }
        }

        private class ClassWithConstructorDependency
        {
            public Class Dependency { get; }

            public ClassWithConstructorDependency(Class dependency)
            {
                Dependency = dependency;
            }
        }

        private class ClassWithMultipleConstructorDependencies
        {
            public Class ClassDependency { get; }
            public IInterface InterfaceDependency { get; }

            public ClassWithMultipleConstructorDependencies(Class classDependency, IInterface interfaceDependency)
            {
                ClassDependency = classDependency;
                InterfaceDependency = interfaceDependency;
            }
        }

        private static void AssertCircularDependency(TestDelegate action)
        {
            Assert.That(action, Throws.Exception.With.Message.Contains("Circular dependency detected"));
        }

        private static void ResolveNonLazyBindings(Container container)
        {
            var method = typeof(Container).GetMethod("ResolveNonLazyBindings", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                method.Invoke(container, null);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

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
        public void Instantiate_WhenTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Instantiate((Type)null),
                Throws.TypeOf<ArgumentNullException>());
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

        [Test]
        public void ResolveNonLazyBindings_WhenTransientBindingIsNonLazy_CreatesInstanceBeforeResolve()
        {
            NonLazyTransientClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyTransientClass>().AsTransient().NonLazy();

            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(0));

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));

            var first = container.Resolve<NonLazyTransientClass>();
            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));

            var second = container.Resolve<NonLazyTransientClass>();
            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenCachedBindingIsNonLazy_CreatesAndCachesInstance()
        {
            NonLazyCachedClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyCachedClass>().AsCached().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyCachedClass.InstancesCount, Is.EqualTo(1));

            var first = container.Resolve<NonLazyCachedClass>();
            var second = container.Resolve<NonLazyCachedClass>();

            Assert.That(NonLazyCachedClass.InstancesCount, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenShortcutNonLazyIsUsed_CreatesTransientInstance()
        {
            NonLazyShortcutClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyShortcutClass>().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyShortcutClass.InstancesCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FromConstructor_WhenDependenciesAreCircular_ThrowsException()
        {
            var container = new Container();
            container.Bind<ConstructorCircularA>();
            container.Bind<ConstructorCircularB>();

            AssertCircularDependency(() => container.Resolve<ConstructorCircularA>());
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteDependencyResolvesContractAgain_ThrowsException()
        {
            var container = new Container();
            container.Bind<IFromResolveCircularDependency>().To<FromResolveCircularDependency>().FromResolve();
            container.Bind<FromResolveCircularDependency>();

            AssertCircularDependency(() => container.Resolve<IFromResolveCircularDependency>());
        }

        [Test]
        public void ResolveNonLazyBindings_WhenNonLazyBindingHasCircularDependencies_ThrowsException()
        {
            var container = new Container();
            container.Bind<ConstructorCircularA>().AsTransient().NonLazy();
            container.Bind<ConstructorCircularB>();

            AssertCircularDependency(() => ResolveNonLazyBindings(container));
        }

        [Test]
        public void Inject_WhenInstanceHasInjectMethod_ResolvesDependencyAndInvokesMethod()
        {
            var container = new Container();
            var dependency = new Class();
            var target = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.Inject(target);

            Assert.That(target.Dependency, Is.SameAs(dependency));
            Assert.That(target.CallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Inject_WhenInjectMethodHasNoParameters_InvokesMethod()
        {
            var container = new Container();
            var target = new ParameterlessInjectableClass();

            container.Inject(target);

            Assert.That(target.WasInjected, Is.True);
        }

        [Test]
        public void Inject_WhenInjectMethodHasMultipleParameters_ResolvesAllDependencies()
        {
            var container = new Container();
            var classDependency = new Class();
            var interfaceDependency = new ClassImplementedIInterface();
            var target = new MultiDependencyInjectableClass();

            container.Bind<Class>().FromInstance(classDependency);
            container.Bind<IInterface>().FromInstance(interfaceDependency);

            container.Inject(target);

            Assert.That(target.ClassDependency, Is.SameAs(classDependency));
            Assert.That(target.InterfaceDependency, Is.SameAs(interfaceDependency));
        }

        [Test]
        public void Inject_WhenInstanceHasNoInjectMethod_DoesNothing()
        {
            var container = new Container();
            var target = new ClassWithoutInjectMethod();

            Assert.That(() => container.Inject(target), Throws.Nothing);
            Assert.That(target.WasConstructCalled, Is.False);
        }

        [Test]
        public void Inject_WhenDependencyIsNotBound_ThrowsException()
        {
            var container = new Container();
            var target = new InjectableClass();

            Assert.That(
                () => container.Inject(target),
                Throws.Exception.With.Message.Contains("No binding found"));
        }

        [Test]
        public void Inject_WhenInstanceHasMultipleInjectMethods_ThrowsInjectException()
        {
            var container = new Container();
            var target = new ClassWithMultipleInjectMethods();

            Assert.That(
                () => container.Inject(target),
                Throws.TypeOf<InjectException>());
        }

        [Test]
        public void Inject_WhenEnumerableIsPassed_InjectsEveryInstance()
        {
            var container = new Container();
            var dependency = new Class();
            var first = new InjectableClass();
            var second = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.Inject((System.Collections.Generic.IEnumerable<object>)new object[] { first, second });

            Assert.That(first.Dependency, Is.SameAs(dependency));
            Assert.That(second.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Instantiate_WhenTypeHasParameterlessConstructor_ReturnsInstance()
        {
            var container = new Container();

            var instance = container.Instantiate<Class>();

            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void Instantiate_WhenTypeHasConstructorDependency_ResolvesDependency()
        {
            var container = new Container();
            var dependency = new Class();

            container.Bind<Class>().FromInstance(dependency);

            var instance = container.Instantiate<ClassWithConstructorDependency>();

            Assert.That(instance.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Instantiate_WhenTypeHasMultipleConstructorDependencies_ResolvesAllDependencies()
        {
            var container = new Container();
            var classDependency = new Class();
            var interfaceDependency = new ClassImplementedIInterface();

            container.Bind<Class>().FromInstance(classDependency);
            container.Bind<IInterface>().FromInstance(interfaceDependency);

            var instance = (ClassWithMultipleConstructorDependencies)container.Instantiate(typeof(ClassWithMultipleConstructorDependencies));

            Assert.That(instance.ClassDependency, Is.SameAs(classDependency));
            Assert.That(instance.InterfaceDependency, Is.SameAs(interfaceDependency));
        }

        [Test]
        public void Instantiate_WhenConstructorDependencyIsNotBound_ThrowsException()
        {
            var container = new Container();

            Assert.That(
                () => container.Instantiate<ClassWithConstructorDependency>(),
                Throws.Exception.With.Message.Contains("No binding found"));
        }

        [Test]
        public void Instantiate_WhenPrefabIsGameObject_ReturnsClonedGameObject()
        {
            var prefab = new GameObject("Prefab");
            var cloned = default(GameObject);

            try
            {
                prefab.AddComponent<Script>();

                var container = new Container();
                cloned = container.Instantiate(prefab);

                Assert.That(cloned, Is.Not.Null);
                Assert.That(cloned, Is.Not.SameAs(prefab));
                Assert.That(cloned.GetComponent<Script>(), Is.Not.Null);
                Assert.That(cloned.GetComponent<Script>(), Is.Not.SameAs(prefab.GetComponent<Script>()));
            }
            finally
            {
                if (cloned != null)
                    UnityEngine.Object.DestroyImmediate(cloned);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Instantiate_WhenPrefabIsComponent_ReturnsClonedComponent()
        {
            var prefabGameObject = new GameObject("Prefab");
            var clonedScript = default(Script);

            try
            {
                var prefabScript = prefabGameObject.AddComponent<Script>();

                var container = new Container();
                clonedScript = container.Instantiate(prefabScript);

                Assert.That(clonedScript, Is.Not.Null);
                Assert.That(clonedScript, Is.Not.SameAs(prefabScript));
                Assert.That(clonedScript.gameObject, Is.Not.SameAs(prefabGameObject));
            }
            finally
            {
                if (clonedScript != null)
                    UnityEngine.Object.DestroyImmediate(clonedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabGameObject);
            }
        }
    }
}
