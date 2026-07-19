using NUnit.Framework;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromNewComponentOnTests : ContainerFactoryTestFixture
    {
        private class InterfaceResultFactory : Factory<IInterface> { }

        [Test]
        public void Create_FromNewComponentOn_AddsAndInjectsNewComponentForEveryCreate()
        {
            var gameObject = new GameObject("Target");
            var dependency = new Class();

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<InjectableScript, InjectableScriptFactory>()
                    .FromNewComponentOn(gameObject)
                    .AsCached();
                var factory = container.Resolve<InjectableScriptFactory>();

                var first = factory.Create();
                var second = factory.Create();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(first.gameObject, Is.SameAs(gameObject));
                Assert.That(second.gameObject, Is.SameAs(gameObject));
                Assert.That(first.Dependency, Is.SameAs(dependency));
                Assert.That(second.Dependency, Is.SameAs(dependency));
                Assert.That(gameObject.GetComponents<InjectableScript>(), Has.Length.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOn_WhenResultContractIsInterface_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, InterfaceResultFactory>()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOn(gameObject)
                    .AsCached();

                var result = container.Resolve<InterfaceResultFactory>().Create();

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That((Component)result, Is.SameAs(gameObject.GetComponent<ScriptImplementedIInterface>()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
