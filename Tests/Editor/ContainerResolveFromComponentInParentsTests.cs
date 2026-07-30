using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class ComponentInParentsScriptTarget : MonoBehaviour
    {
        public Script Script { get; private set; }

        [Inject]
        public void Construct(Script script)
        {
            Script = script;
        }
    }

    public sealed class ComponentInParentsInterfaceTarget : MonoBehaviour
    {
        public IInterface Service { get; private set; }

        [Inject]
        public void Construct(IInterface service)
        {
            Service = service;
        }
    }

    public sealed class ComponentInParentsAbstractTarget : MonoBehaviour
    {
        public AbstractScript Script { get; private set; }

        [Inject]
        public void Construct(AbstractScript script)
        {
            Script = script;
        }
    }

    public sealed class ComponentInParentsInjectionProbeTarget : MonoBehaviour
    {
        public ComponentInParentsInjectionProbe Probe { get; private set; }

        [Inject]
        public void Construct(ComponentInParentsInjectionProbe probe)
        {
            Probe = probe;
        }
    }

    public sealed class ComponentInParentsConcreteAbstractScript : AbstractScript
    {
    }

    public sealed class ComponentInParentsInjectionProbe : MonoBehaviour
    {
        public int InjectionCount { get; private set; }

        [Inject]
        public void Construct()
        {
            InjectionCount++;
        }
    }

    public class ContainerResolveFromComponentInParentsTests
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
        public void Bind_FromComponentInParents_WhenConcreteTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromComponentInParents(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromComponentInParents_WhenInterfaceMapsToPlainClass_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>()
                    .To<ClassImplementedIInterface>()
                    .FromComponentInParents(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Inject_FromComponentInParents_ReturnsExistingParentComponentWithoutAddingAnother()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
                Assert.That(parentObject.GetComponents<Script>(), Has.Length.EqualTo(1));
                Assert.That(consumerObject.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenContractIsInterface_ReturnsImplementingComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInParentsInterfaceTarget>();
                var container = new Container();
                container.Bind<IInterface>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInParentsInterfaceTarget>();
                var container = new Container();
                container.Bind<IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenBindingIsNonGeneric_ReturnsImplementingComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInParentsInterfaceTarget>();
                var container = new Container();
                container.Bind(typeof(IInterface)).FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenNonGenericInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInParentsInterfaceTarget>();
                var container = new Container();
                container.Bind(typeof(IInterface))
                    .To(typeof(ScriptImplementedIInterface))
                    .FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenContractIsAbstractComponent_ReturnsDerivedComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var existing = parentObject.AddComponent<ComponentInParentsConcreteAbstractScript>();
                var consumer = consumerObject.AddComponent<ComponentInParentsAbstractTarget>();
                var container = new Container();
                container.Bind<AbstractScript>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenConsumerHasComponent_PrefersItOverParent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var expected = consumerObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenSeveralParentsHaveComponent_PrefersNearest()
        {
            var rootObject = new GameObject("Root");
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                parentObject.transform.SetParent(rootObject.transform);
                consumerObject.transform.SetParent(parentObject.transform);
                rootObject.AddComponent<Script>();
                var expected = parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(rootObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenOnlyGrandparentHasComponent_ReturnsIt()
        {
            var rootObject = new GameObject("Root");
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                parentObject.transform.SetParent(rootObject.transform);
                consumerObject.transform.SetParent(parentObject.transform);
                var expected = rootObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(rootObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenParentIsInactive_ReturnsItsComponent()
        {
            var parentObject = new GameObject("InactiveParent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var expected = parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                parentObject.SetActive(false);
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_CrossesGameObjectContextBoundary()
        {
            var outerObject = new GameObject("Outer");
            var contextObject = new GameObject("Context");
            var consumerObject = new GameObject("Consumer");

            try
            {
                contextObject.transform.SetParent(outerObject.transform);
                consumerObject.transform.SetParent(contextObject.transform);
                var expected = outerObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInParents();

                context.Container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(outerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_DoesNotSearchChildrenOrSiblings()
        {
            var rootObject = new GameObject("Root");
            var consumerObject = new GameObject("Consumer");
            var siblingObject = new GameObject("Sibling");
            var childObject = new GameObject("Child");

            try
            {
                consumerObject.transform.SetParent(rootObject.transform);
                siblingObject.transform.SetParent(rootObject.transform);
                childObject.transform.SetParent(consumerObject.transform);
                siblingObject.AddComponent<Script>();
                childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(rootObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenNearestObjectHasSeveralComponents_ReturnsFirst()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var expected = parentObject.AddComponent<Script>();
                parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenComponentIsMissing_ReportsTypeAndConsumer()
        {
            var gameObject = new GameObject("Consumer");

            try
            {
                var consumer = gameObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                var exception = Assert.Throws<InvalidOperationException>(
                    () => container.Inject(consumer));

                Assert.That(exception.Message, Does.Contain(typeof(Script).ToString()));
                Assert.That(exception.Message, Does.Contain(gameObject.name));
                Assert.That(
                    exception.Message,
                    Does.Contain(typeof(ComponentInParentsScriptTarget).ToString()));
            }
            finally
            {
                DestroyGameObjects(gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInParents_WithoutConsumer_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInParents();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Instantiate_FromComponentInParents_WithoutConsumerInstance_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInParents();

            Assert.That(
                () => container.Instantiate<ConstructorConsumer>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInParents_WhenConsumerIsNotMonoBehaviour_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInParents();

            Assert.That(
                () => container.Inject(new PlainConsumer()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInParents_WhenConsumerIsDestroyed_ThrowsInvalidOperationException()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents();

                UnityEngine.Object.DestroyImmediate(consumer);

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_AsTransient_RepeatsSearchForSameConsumer()
        {
            var firstParentObject = new GameObject("FirstParent");
            var secondParentObject = new GameObject("SecondParent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(firstParentObject.transform);
                var firstComponent = firstParentObject.AddComponent<Script>();
                var secondComponent = secondParentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents().AsTransient();

                container.Inject(consumer);
                var firstResolved = consumer.Script;
                consumerObject.transform.SetParent(secondParentObject.transform);
                container.Inject(consumer);

                Assert.That(firstResolved, Is.SameAs(firstComponent));
                Assert.That(consumer.Script, Is.SameAs(secondComponent));
            }
            finally
            {
                DestroyGameObjects(firstParentObject, secondParentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_AsCached_UsesFirstSuccessfulConsumerComponent()
        {
            var firstParentObject = new GameObject("FirstParent");
            var firstConsumerObject = new GameObject("FirstConsumer");
            var secondParentObject = new GameObject("SecondParent");
            var secondConsumerObject = new GameObject("SecondConsumer");

            try
            {
                firstConsumerObject.transform.SetParent(firstParentObject.transform);
                secondConsumerObject.transform.SetParent(secondParentObject.transform);
                var firstComponent = firstParentObject.AddComponent<Script>();
                var secondComponent = secondParentObject.AddComponent<Script>();
                var firstConsumer = firstConsumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var secondConsumer = secondConsumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents().AsCached();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(firstConsumer.Script, Is.SameAs(firstComponent));
                Assert.That(secondConsumer.Script, Is.SameAs(firstComponent));
                Assert.That(secondConsumer.Script, Is.Not.SameAs(secondComponent));
            }
            finally
            {
                DestroyGameObjects(firstParentObject, secondParentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_AsCached_DoesNotCacheFailedSearch()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInParents().AsCached();

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());

                var expected = parentObject.AddComponent<Script>();
                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Inject_FromComponentInParents_WhenBindingComesFromParentContainer_UsesOriginalConsumer()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var expected = parentObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInParentsScriptTarget>();
                var bindingOwner = new Container();
                bindingOwner.Bind<Script>().FromComponentInParents();
                var injectingContainer = new Container(bindingOwner);

                injectingContainer.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Build_FromComponentInParents_NonLazy_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInParents().AsTransient().NonLazy();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Build_FromComponentInParents_AsEntryPoint_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<ScriptWithEntryPoint>().FromComponentInParents().AsEntryPoint();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInParents_DoesNotInjectFoundComponent()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                var probe = parentObject.AddComponent<ComponentInParentsInjectionProbe>();
                var consumer =
                    consumerObject.AddComponent<ComponentInParentsInjectionProbeTarget>();
                var container = new Container();
                container.Bind<ComponentInParentsInjectionProbe>()
                    .FromComponentInParents();

                container.Inject(consumer);

                Assert.That(consumer.Probe, Is.SameAs(probe));
                Assert.That(probe.InjectionCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        private static void DestroyGameObjects(params GameObject[] gameObjects)
        {
            for (var i = gameObjects.Length - 1; i >= 0; i--)
            {
                if (gameObjects[i] != null)
                    UnityEngine.Object.DestroyImmediate(gameObjects[i]);
            }
        }
    }
}
