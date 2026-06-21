using System;
using System.Reflection;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryTests
    {
        private interface IProduct { }

        private class Product : IProduct { }

        private class ProductDependency
        {
            public Product Product { get; } = new();
        }

        private class ProductFactory : Factory<Product> { }

        private class InterfaceProductFactory : Factory<IProduct> { }

        private class ScriptFactory : Factory<Script> { }

        private class InjectableScriptFactory : Factory<InjectableScript> { }

        private class CustomProductFactory : CustomFactory<Product>
        {
            private ProductDependency _dependency;

            [Inject]
            private void Construct(ProductDependency dependency)
            {
                _dependency = dependency;
            }

            public override Product Create()
            {
                return _dependency.Product;
            }
        }

        private class CustomScriptWithParameterFactory : CustomFactory<GameObject, Script>
        {
            public override Script Create(GameObject prefab)
            {
                return new GameObject(prefab.name).AddComponent<Script>();
            }
        }

        private class CustomInterfaceScriptWithParameterFactory : CustomFactory<GameObject, ScriptImplementedIInterface>
        {
            public override ScriptImplementedIInterface Create(GameObject prefab)
            {
                return new GameObject(prefab.name).AddComponent<ScriptImplementedIInterface>();
            }
        }

        private class CustomScriptWithInterfaceParameterFactory : CustomFactory<IInterface, Script>
        {
            public override Script Create(IInterface prefab)
            {
                var prefabComponent = (Component)prefab;
                return new GameObject(prefabComponent.gameObject.name).AddComponent<Script>();
            }
        }

        private class CustomInjectableScriptWithParameterFactory : CustomFactory<GameObject, InjectableScript>
        {
            public override InjectableScript Create(GameObject prefab)
            {
                var gameObject = new GameObject(prefab.name);
                return _objectBuilder.AddComponent<InjectableScript>(gameObject);
            }
        }

        private static void InjectQueuedInstances(Container container)
        {
            var method = typeof(Container).GetMethod("InjectQueuedInstances", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(container, null);
        }

        [Test]
        public void BindFactory_WhenFactoryWasBound_ResolvesFactoryByFactoryType()
        {
            var container = new Container();
            container.Bind<Product, ProductFactory>().FromConstructor().AsTransient();

            var factory = container.Resolve<ProductFactory>();
            var product = factory.Create();

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory is IFactory<Product>, Is.True);
            Assert.That(product, Is.TypeOf<Product>());
        }

        [Test]
        public void ResolveFactory_AsTransient_ReturnsDifferentFactories()
        {
            var container = new Container();
            container.Bind<Product, ProductFactory>().FromConstructor().AsTransient();

            var first = container.Resolve<ProductFactory>();
            var second = container.Resolve<ProductFactory>();

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void ResolveFactory_AsCached_ReturnsSameFactoryButCreatesTransientResults()
        {
            var container = new Container();
            container.Bind<Product, ProductFactory>().FromConstructor().AsCached();

            var firstFactory = container.Resolve<ProductFactory>();
            var secondFactory = container.Resolve<ProductFactory>();
            var firstProduct = firstFactory.Create();
            var secondProduct = secondFactory.Create();

            Assert.That(secondFactory, Is.SameAs(firstFactory));
            Assert.That(firstProduct, Is.Not.SameAs(secondProduct));
        }

        [Test]
        public void Create_WhenFactoryUsesConcreteResultType_ReturnsConcreteResult()
        {
            var container = new Container();
            container.Bind<IProduct, InterfaceProductFactory>().To<Product>().FromConstructor().AsTransient();

            var factory = container.Resolve<InterfaceProductFactory>();
            var product = factory.Create();

            Assert.That(product, Is.TypeOf<Product>());
        }

        [Test]
        public void Create_FromResolve_ReturnsResolvedResult()
        {
            var container = new Container();
            container.Bind<Product>().AsCached();
            container.Bind<Product, ProductFactory>().FromResolve().AsTransient();

            var factory = container.Resolve<ProductFactory>();
            var first = factory.Create();
            var second = factory.Create();
            var resolved = container.Resolve<Product>();

            Assert.That(second, Is.SameAs(first));
            Assert.That(resolved, Is.SameAs(first));
        }

        [Test]
        public void Create_FromComponentInNewPrefab_WhenPrefabIsGameObject_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script, ScriptFactory>().FromComponentInNewPrefab(prefabScript.gameObject).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_FromComponentInNewPrefab_WhenPrefabIsComponent_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script, ScriptFactory>().FromComponentInNewPrefab(prefabScript).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnNewPrefab_WhenPrefabIsGameObject_AddsComponentToClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script, ScriptFactory>().FromNewComponentOnNewPrefab(prefab).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefab));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_FromNewComponentOnNewPrefab_WhenPrefabIsComponent_AddsComponentToClonedPrefab()
        {
            var prefabComponent = new GameObject("Prefab").transform;
            var result = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script, ScriptFactory>().FromNewComponentOnNewPrefab(prefabComponent).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabComponent.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabComponent.gameObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnNewGameObject_AddsComponentAndInjectsIt()
        {
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.Bind<InjectableScript, InjectableScriptFactory>().FromNewComponentOnNewGameObject().AsTransient();

                result = container.Resolve<InjectableScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);
            }
        }

        [Test]
        public void Create_FromFactory_UsesInjectedCustomFactory()
        {
            var dependency = new ProductDependency();
            var container = new Container();
            container.Bind<ProductDependency>().FromInstance(dependency);
            container.Bind<Product, ProductFactory>().To<Product>().FromFactory<CustomProductFactory>().AsTransient();

            InjectQueuedInstances(container);
            var product = container.Resolve<ProductFactory>().Create();

            Assert.That(product, Is.SameAs(dependency.Product));
        }

        [Test]
        public void BindFactoryWithParameter_WhenFactoryWasBound_ResolvesFactoryByFactoryType()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, Script>>();
                result = factory.Create(prefabScript.gameObject);

                Assert.That(factory, Is.Not.Null);
                Assert.That(factory is IFactory<GameObject, Script>, Is.True);
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void ResolveFactoryWithParameter_AsTransient_ReturnsDifferentFactories()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script>().FromComponentInNewPrefab().AsTransient();

            var first = container.Resolve<Factory<GameObject, Script>>();
            var second = container.Resolve<Factory<GameObject, Script>>();

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void ResolveFactoryWithParameter_AsCached_ReturnsSameFactoryButCreatesTransientResults()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var firstResult = default(Script);
            var secondResult = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>().FromComponentInNewPrefab().AsCached();

                var firstFactory = container.Resolve<Factory<GameObject, Script>>();
                var secondFactory = container.Resolve<Factory<GameObject, Script>>();
                firstResult = firstFactory.Create(prefabScript.gameObject);
                secondResult = secondFactory.Create(prefabScript.gameObject);

                Assert.That(secondFactory, Is.SameAs(firstFactory));
                Assert.That(firstResult, Is.Not.SameAs(secondResult));
                Assert.That(firstResult.gameObject, Is.Not.SameAs(secondResult.gameObject));
            }
            finally
            {
                if (firstResult != null)
                    UnityEngine.Object.DestroyImmediate(firstResult.gameObject);

                if (secondResult != null)
                    UnityEngine.Object.DestroyImmediate(secondResult.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<Factory<GameObject, IInterface>>();
                result = factory.Create(prefabScript.gameObject);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabParameterIsComponent_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, Script>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<Script, Script>>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabParameterIsInterface_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<Factory<IInterface, IInterface>>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsGameObject_AddsComponentToClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, Script>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefab));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsComponent_AddsComponentToClonedPrefab()
        {
            var prefabComponent = new GameObject("Prefab").transform;
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Transform, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<Transform, Script>>();
                result = factory.Create(prefabComponent);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabComponent.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefabComponent.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabComponent.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabParameterIsInterface_AddsComponentToClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<IInterface, Script>>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefabScript.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefabAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefab = new GameObject("Prefab");
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOnNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<Factory<GameObject, IInterface>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefab));
                Assert.That(prefab.GetComponent<ScriptImplementedIInterface>(), Is.Null);
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_InjectsAddedComponent()
        {
            var prefab = new GameObject("Prefab");
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<GameObject, InjectableScript>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, InjectableScript>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenParameterTypeIsNotPrefabType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<float, Script>().FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenConcreteResultTypeCanNotBeAddedAsComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, IProduct>()
                    .To<Product>()
                    .FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenResultTypeIsInterfaceWithoutConcreteType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, IInterface>().FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script>().FromNewComponentOnNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<GameObject, Script>>();

            Assert.That(
                () => factory.Create(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenInterfaceParameterIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.BindFactory<IInterface, Script>().FromNewComponentOnNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<IInterface, Script>>();

            Assert.That(
                () => factory.Create(new ClassImplementedIInterface()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Create_WithParameterFactoryFromFactory_UsesCustomFactory()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>()
                    .To<Script>()
                    .FromFactory<CustomScriptWithParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<Factory<GameObject, Script>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefab));
                Assert.That(result.gameObject.name, Is.EqualTo(prefab.name));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromFactoryAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefab = new GameObject("Prefab");
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromFactory<CustomInterfaceScriptWithParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<Factory<GameObject, IInterface>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefab));
                Assert.That(((Component)result).gameObject.name, Is.EqualTo(prefab.name));
                Assert.That(prefab.GetComponent<ScriptImplementedIInterface>(), Is.Null);
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromFactory_WhenParameterIsInterface_UsesCustomFactory()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, Script>()
                    .To<Script>()
                    .FromFactory<CustomScriptWithInterfaceParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<Factory<IInterface, Script>>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
                Assert.That(result.gameObject.name, Is.EqualTo(prefabScript.gameObject.name));
                Assert.That(prefabScript.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromFactory_UsesInjectedCustomFactory()
        {
            var prefab = new GameObject("Prefab");
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<GameObject, InjectableScript>()
                    .To<InjectableScript>()
                    .FromFactory<CustomInjectableScriptWithParameterFactory>()
                    .AsTransient();

                InjectQueuedInstances(container);
                var factory = container.Resolve<Factory<GameObject, InjectableScript>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject.name, Is.EqualTo(prefab.name));
                Assert.That(result.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void BindFactoryWithParameter_WhenParameterTypeIsNotPrefabType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<float, Script>().FromComponentInNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameter_WhenResultTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, Product>().FromComponentInNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script>().FromComponentInNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<GameObject, Script>>();

            Assert.That(
                () => factory.Create(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabDoesNotHaveRequestedComponent_ThrowsArgumentException()
        {
            var prefab = new GameObject("Prefab");

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, Script>>();

                Assert.That(
                    () => factory.Create(prefab),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactory_WhenInterfaceParameterIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.BindFactory<IInterface, Script>().FromComponentInNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<IInterface, Script>>();

            Assert.That(
                () => factory.Create(new ClassImplementedIInterface()),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactory_WhenFactoryTypeAlreadyBoundByFactoryBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Product, ProductFactory>();

            Assert.That(
                () => container.Bind<Product, ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void BindFactory_WhenFactoryTypeAlreadyBoundByRegularBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<ProductFactory>();

            Assert.That(
                () => container.Bind<Product, ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Bind_WhenFactoryTypeAlreadyBoundByFactoryBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Product, ProductFactory>();

            Assert.That(
                () => container.Bind<ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_WhenContainerIsCreated_ReturnsContainerAndObjectBuilder()
        {
            var container = new Container();

            Assert.That(container.Resolve<Container>(), Is.SameAs(container));
            Assert.That(container.Resolve<IObjectBuilder>(), Is.SameAs(container));
        }
    }
}
