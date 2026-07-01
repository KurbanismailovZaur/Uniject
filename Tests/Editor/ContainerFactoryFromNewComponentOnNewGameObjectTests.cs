using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromNewComponentOnNewGameObjectTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromNewComponentOnNewGameObject_AddsComponentAndInjectsIt()
        {
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<InjectableScript, InjectableScriptFactory>().FromNewComponentOnNewGameObject().AsTransient();

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
    }
}
