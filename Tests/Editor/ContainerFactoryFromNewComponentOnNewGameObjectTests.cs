using System;
using NUnit.Framework;
using Uniject;
using Uniject.Contexts;
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

        [Test]
        public void Create_FromNewComponentOnNewGameObject_IgnoresContainerParentAndContext()
        {
            var contextObject = new GameObject("GameObjectContext");
            var containerParent = new GameObject("ContainerParent").transform;
            var result = default(Script);

            try
            {
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context, parentTransformForGameObjects: containerParent);
                context.Initialize();
                context.Install();
                context.Container.BindFactory<Script, ScriptFactory>()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();

                result = context.Container.Resolve<ScriptFactory>().Create();

                Assert.That(result.transform.parent, Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(containerParent.gameObject);
                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }
    }
}
