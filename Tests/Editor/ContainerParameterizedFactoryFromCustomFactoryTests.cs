using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryFromCustomFactoryTests : ContainerFactoryTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            InitializableCustomScriptWithParameterFactory.Reset();
            CustomProductWithClassParameterFactory.Reset();
        }

        [Test]
        public void Create_WithParameterFactoryFromFactory_UsesCustomFactory()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script, GameObjectScriptFactory>()
                    .To<Script>()
                    .FromFactory<CustomScriptWithParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<GameObjectScriptFactory>();
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
        public void Create_FromFactory_WhenParameterAndResultAreClasses_UsesCustomFactory()
        {
            var parameter = new Class();
            var container = new Container();
            container.BindFactory<Class, Product, ClassProductFactory>()
                .FromFactory<CustomProductWithClassParameterFactory>()
                .AsCached();

            var factory = container.Resolve<ClassProductFactory>();
            var result = factory.Create(parameter);

            Assert.That(result, Is.TypeOf<Product>());
            Assert.That(CustomProductWithClassParameterFactory.LastParameter, Is.SameAs(parameter));
        }

        [Test]
        public void Create_WithParameterFactoryFromFactoryAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefab = new GameObject("Prefab");
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface, GameObjectInterfaceFactory>()
                    .To<ScriptImplementedIInterface>()
                    .FromFactory<CustomInterfaceScriptWithParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<GameObjectInterfaceFactory>();
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
                container.BindFactory<IInterface, Script, InterfaceScriptFactory>()
                    .To<Script>()
                    .FromFactory<CustomScriptWithInterfaceParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<InterfaceScriptFactory>();
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
        public void Create_WithParameterFactoryFromFactory_UsesCustomFactoryObjectBuilder()
        {
            var prefab = new GameObject("Prefab");
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<GameObject, InjectableScript, GameObjectInjectableScriptFactory>()
                    .To<InjectableScript>()
                    .FromFactory<CustomInjectableScriptWithParameterFactory>()
                    .AsTransient();

                var factory = container.Resolve<GameObjectInjectableScriptFactory>();
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
        public void FromFactory_WithParameter_InitializesCustomFactoryOnceBeforeCreate()
        {
            var prefab = new GameObject("Prefab");
            var firstResult = default(Script);
            var secondResult = default(Script);

            try
            {
                var dependency = new Class();
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<GameObject, Script, GameObjectScriptFactory>()
                    .To<Script>()
                    .FromFactory<InitializableCustomScriptWithParameterFactory>()
                    .AsTransient();

                Assert.That(InitializableCustomScriptWithParameterFactory.InitializeCallsCount, Is.EqualTo(1));
                Assert.That(InitializableCustomScriptWithParameterFactory.CreateCallsCount, Is.Zero);
                Assert.That(InitializableCustomScriptWithParameterFactory.ResolvedDependency, Is.SameAs(dependency));

                var firstFactory = container.Resolve<GameObjectScriptFactory>();
                var secondFactory = container.Resolve<GameObjectScriptFactory>();
                firstResult = firstFactory.Create(prefab);
                secondResult = secondFactory.Create(prefab);

                Assert.That(InitializableCustomScriptWithParameterFactory.InitializeCallsCount, Is.EqualTo(1));
                Assert.That(InitializableCustomScriptWithParameterFactory.CreateCallsCount, Is.EqualTo(2));
                Assert.That(InitializableCustomScriptWithParameterFactory.WasInitializedBeforeCreate, Is.True);
            }
            finally
            {
                if (firstResult != null)
                    UnityEngine.Object.DestroyImmediate(firstResult.gameObject);

                if (secondResult != null)
                    UnityEngine.Object.DestroyImmediate(secondResult.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }
    }
}
