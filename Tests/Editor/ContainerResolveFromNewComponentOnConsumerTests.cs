using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class NewComponentOnConsumerScriptTarget : MonoBehaviour
    {
        public Script Script { get; private set; }

        [Inject]
        public void Construct(Script script)
        {
            Script = script;
        }
    }

    public sealed class NewComponentOnConsumerInterfaceTarget : MonoBehaviour
    {
        public IInterface Service { get; private set; }

        [Inject]
        public void Construct(IInterface service)
        {
            Service = service;
        }
    }

    public sealed class NewComponentOnConsumerInjectableScriptTarget : MonoBehaviour
    {
        public InjectableScript Script { get; private set; }

        [Inject]
        public void Construct(InjectableScript script)
        {
            Script = script;
        }
    }

    public class ContainerResolveFromNewComponentOnConsumerTests
    {
        private sealed class PlainConsumer
        {
            public Script Script { get; private set; }

            [Inject]
            public void Construct(Script script)
            {
                Script = script;
            }
        }

        private sealed class ConstructorConsumer
        {
            public Script Script { get; }

            public ConstructorConsumer(Script script)
            {
                Script = script;
            }
        }

        [Test]
        public void Bind_FromNewComponentOnConsumer_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromNewComponentOnConsumer(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnConsumer_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().FromNewComponentOnConsumer(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnConsumer_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<AbstractScript>().FromNewComponentOnConsumer(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_AddsAndReturnsComponentOnConsumerGameObject()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<NewComponentOnConsumerScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromNewComponentOnConsumer();

                Assert.That(gameObject.GetComponent<Script>(), Is.Null);

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.Not.Null);
                Assert.That(consumer.Script.gameObject, Is.SameAs(gameObject));
                Assert.That(gameObject.GetComponent<Script>(), Is.SameAs(consumer.Script));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_InjectsAddedComponent()
        {
            var gameObject = new GameObject("Consumer");
            var dependency = new Class();

            try
            {
                var consumer = gameObject.AddComponent<NewComponentOnConsumerInjectableScriptTarget>();
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.Bind<InjectableScript>().FromNewComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Script.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_WhenContractIsInterface_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<NewComponentOnConsumerInterfaceTarget>();
                var container = new Container();
                container.Bind<IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(((Component)consumer.Service).gameObject, Is.SameAs(gameObject));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_WhenBindingIsNonGeneric_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<NewComponentOnConsumerInterfaceTarget>();
                var container = new Container();
                container.Bind(typeof(IInterface))
                    .To(typeof(ScriptImplementedIInterface))
                    .FromNewComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(((Component)consumer.Service).gameObject, Is.SameAs(gameObject));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnConsumer_WithoutConsumer_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromNewComponentOnConsumer();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Instantiate_FromNewComponentOnConsumer_WithoutConsumerInstance_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromNewComponentOnConsumer();

            Assert.That(
                () => container.Instantiate<ConstructorConsumer>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_WhenConsumerIsNotMonoBehaviour_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromNewComponentOnConsumer();

            Assert.That(
                () => container.Inject(new PlainConsumer()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_AsTransient_AddsComponentForEachConsumer()
        {
            var firstGameObject = new GameObject("FirstConsumer");
            var secondGameObject = new GameObject("SecondConsumer");

            try
            {
                var firstConsumer = firstGameObject.AddComponent<NewComponentOnConsumerScriptTarget>();
                var secondConsumer = secondGameObject.AddComponent<NewComponentOnConsumerScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromNewComponentOnConsumer().AsTransient();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(secondConsumer.Script, Is.Not.SameAs(firstConsumer.Script));
                Assert.That(firstConsumer.Script.gameObject, Is.SameAs(firstGameObject));
                Assert.That(secondConsumer.Script.gameObject, Is.SameAs(secondGameObject));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstGameObject);
                UnityEngine.Object.DestroyImmediate(secondGameObject);
            }
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_AsCached_UsesFirstConsumerGameObject()
        {
            var firstGameObject = new GameObject("FirstConsumer");
            var secondGameObject = new GameObject("SecondConsumer");

            try
            {
                var firstConsumer = firstGameObject.AddComponent<NewComponentOnConsumerScriptTarget>();
                var secondConsumer = secondGameObject.AddComponent<NewComponentOnConsumerScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromNewComponentOnConsumer().AsCached();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(secondConsumer.Script, Is.SameAs(firstConsumer.Script));
                Assert.That(firstConsumer.Script.gameObject, Is.SameAs(firstGameObject));
                Assert.That(secondGameObject.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstGameObject);
                UnityEngine.Object.DestroyImmediate(secondGameObject);
            }
        }

        [Test]
        public void Build_FromNewComponentOnConsumer_NonLazy_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromNewComponentOnConsumer().AsTransient().NonLazy();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromNewComponentOnConsumer_WhenBindingComesFromParent_UsesParentContainer()
        {
            var gameObject = new GameObject("Consumer");
            var dependency = new Class();

            try
            {
                var consumer = gameObject.AddComponent<NewComponentOnConsumerInjectableScriptTarget>();
                var parent = new Container();
                parent.Bind<Class>().FromInstance(dependency);
                parent.Bind<InjectableScript>().FromNewComponentOnConsumer();
                var child = new Container(parent);

                child.Inject(consumer);

                Assert.That(consumer.Script.gameObject, Is.SameAs(gameObject));
                Assert.That(consumer.Script.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
