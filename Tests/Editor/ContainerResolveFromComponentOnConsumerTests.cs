using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class ComponentOnConsumerScriptTarget : MonoBehaviour
    {
        public Script Script { get; private set; }

        [Inject]
        public void Construct(Script script)
        {
            Script = script;
        }
    }

    public sealed class ComponentOnConsumerInterfaceTarget : MonoBehaviour
    {
        public IInterface Service { get; private set; }

        [Inject]
        public void Construct(IInterface service)
        {
            Service = service;
        }
    }

    public sealed class ComponentOnConsumerAbstractTarget : MonoBehaviour
    {
        public AbstractScript Script { get; private set; }

        [Inject]
        public void Construct(AbstractScript script)
        {
            Script = script;
        }
    }

    public sealed class ComponentOnConsumerInjectionProbeTarget : MonoBehaviour
    {
        public ComponentOnConsumerInjectionProbe Probe { get; private set; }

        [Inject]
        public void Construct(ComponentOnConsumerInjectionProbe probe)
        {
            Probe = probe;
        }
    }

    public sealed class ComponentOnConsumerConcreteAbstractScript : AbstractScript
    {
    }

    public sealed class ComponentOnConsumerInjectionProbe : MonoBehaviour
    {
        public int InjectionCount { get; private set; }

        [Inject]
        public void Construct()
        {
            InjectionCount++;
        }
    }

    public class ContainerResolveFromComponentOnConsumerTests
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
        public void Bind_FromComponentOnConsumer_WhenConcreteTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromComponentOnConsumer(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Inject_FromComponentOnConsumer_ReturnsExistingComponentWithoutAddingAnother()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var existing = gameObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
                Assert.That(gameObject.GetComponents<Script>(), Has.Length.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenContractIsInterface_ReturnsImplementingComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerInterfaceTarget>();
                var existing = gameObject.AddComponent<ScriptImplementedIInterface>();
                var container = new Container();
                container.Bind<IInterface>().FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerInterfaceTarget>();
                var existing = gameObject.AddComponent<ScriptImplementedIInterface>();
                var container = new Container();
                container.Bind<IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenBindingIsNonGeneric_ReturnsComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerInterfaceTarget>();
                var existing = gameObject.AddComponent<ScriptImplementedIInterface>();
                var container = new Container();
                container.Bind(typeof(IInterface)).FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenContractIsAbstractComponent_ReturnsDerivedComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerAbstractTarget>();
                var existing = gameObject.AddComponent<ComponentOnConsumerConcreteAbstractScript>();
                var container = new Container();
                container.Bind<AbstractScript>().FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenComponentIsMissing_ThrowsInvalidOperationException()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer();

                var exception = Assert.Throws<InvalidOperationException>(() => container.Inject(consumer));

                Assert.That(exception.Message, Does.Contain(typeof(Script).ToString()));
                Assert.That(exception.Message, Does.Contain(gameObject.name));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentOnConsumer_WithoutConsumer_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentOnConsumer();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Instantiate_FromComponentOnConsumer_WithoutConsumerInstance_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentOnConsumer();

            Assert.That(
                () => container.Instantiate<ConstructorConsumer>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenConsumerIsNotMonoBehaviour_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentOnConsumer();

            Assert.That(
                () => container.Inject(new PlainConsumer()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenConsumerIsDestroyed_ThrowsInvalidOperationException()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                gameObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer();

                UnityEngine.Object.DestroyImmediate(consumer);

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_DoesNotSearchParentsOrChildren()
        {
            var parent = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");
            var child = new GameObject("Child");

            try
            {
                consumerObject.transform.SetParent(parent.transform);
                child.transform.SetParent(consumerObject.transform);
                parent.AddComponent<Script>();
                child.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer();

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_WhenMultipleComponentsMatch_ReturnsFirst()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var first = gameObject.AddComponent<Script>();
                gameObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(first));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_AsTransient_UsesEachConsumerComponent()
        {
            var firstGameObject = new GameObject("FirstConsumer");
            var secondGameObject = new GameObject("SecondConsumer");

            try
            {
                var firstConsumer = firstGameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var firstScript = firstGameObject.AddComponent<Script>();
                var secondConsumer = secondGameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var secondScript = secondGameObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer().AsTransient();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(firstConsumer.Script, Is.SameAs(firstScript));
                Assert.That(secondConsumer.Script, Is.SameAs(secondScript));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstGameObject);
                UnityEngine.Object.DestroyImmediate(secondGameObject);
            }
        }

        [Test]
        public void Inject_FromComponentOnConsumer_AsCached_UsesFirstConsumerComponent()
        {
            var firstGameObject = new GameObject("FirstConsumer");
            var secondGameObject = new GameObject("SecondConsumer");

            try
            {
                var firstConsumer = firstGameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var firstScript = firstGameObject.AddComponent<Script>();
                var secondConsumer = secondGameObject.AddComponent<ComponentOnConsumerScriptTarget>();
                var secondScript = secondGameObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentOnConsumer().AsCached();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(firstConsumer.Script, Is.SameAs(firstScript));
                Assert.That(secondConsumer.Script, Is.SameAs(firstScript));
                Assert.That(secondConsumer.Script, Is.Not.SameAs(secondScript));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstGameObject);
                UnityEngine.Object.DestroyImmediate(secondGameObject);
            }
        }

        [Test]
        public void Build_FromComponentOnConsumer_NonLazy_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentOnConsumer().AsTransient().NonLazy();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentOnConsumer_DoesNotInjectFoundComponent()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentOnConsumerInjectionProbeTarget>();
                var probe = gameObject.AddComponent<ComponentOnConsumerInjectionProbe>();
                var container = new Container();
                container.Bind<ComponentOnConsumerInjectionProbe>().FromComponentOnConsumer();

                container.Inject(consumer);

                Assert.That(consumer.Probe, Is.SameAs(probe));
                Assert.That(probe.InjectionCount, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
