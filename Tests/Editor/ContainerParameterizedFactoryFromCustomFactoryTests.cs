using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryFromCustomFactoryTests : ContainerFactoryTestFixture
    {
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
    }
}
