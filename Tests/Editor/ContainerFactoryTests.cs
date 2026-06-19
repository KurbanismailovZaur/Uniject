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

        private class CustomProductFactory : IFactory<Product>
        {
            private ProductDependency _dependency;

            [Inject]
            private void Construct(ProductDependency dependency)
            {
                _dependency = dependency;
            }

            public Product Create()
            {
                return _dependency.Product;
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
