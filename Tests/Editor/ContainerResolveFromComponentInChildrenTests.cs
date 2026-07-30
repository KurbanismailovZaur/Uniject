using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class ComponentInChildrenScriptTarget : MonoBehaviour
    {
        public Script Script { get; private set; }

        [Inject]
        public void Construct(Script script)
        {
            Script = script;
        }
    }

    public sealed class ComponentInChildrenInterfaceTarget : MonoBehaviour
    {
        public IInterface Service { get; private set; }

        [Inject]
        public void Construct(IInterface service)
        {
            Service = service;
        }
    }

    public sealed class ComponentInChildrenAbstractTarget : MonoBehaviour
    {
        public AbstractScript Script { get; private set; }

        [Inject]
        public void Construct(AbstractScript script)
        {
            Script = script;
        }
    }

    public sealed class ComponentInChildrenInjectionProbeTarget : MonoBehaviour
    {
        public ComponentInChildrenInjectionProbe Probe { get; private set; }

        [Inject]
        public void Construct(ComponentInChildrenInjectionProbe probe)
        {
            Probe = probe;
        }
    }

    public sealed class ComponentInChildrenConcreteAbstractScript : AbstractScript
    {
    }

    public sealed class ComponentInChildrenInjectionProbe : MonoBehaviour
    {
        public int InjectionCount { get; private set; }

        [Inject]
        public void Construct()
        {
            InjectionCount++;
        }
    }

    public class ContainerResolveFromComponentInChildrenTests
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
        public void Bind_FromComponentInChildren_WhenConcreteTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromComponentInChildren(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromComponentInChildren_WhenInterfaceMapsToPlainClass_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>()
                    .To<ClassImplementedIInterface>()
                    .FromComponentInChildren(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Inject_FromComponentInChildren_ReturnsExistingChildComponentWithoutAddingAnother()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing = childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
                Assert.That(
                    consumerObject.GetComponentsInChildren<Script>(true),
                    Has.Length.EqualTo(1));
                Assert.That(consumerObject.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenContractIsInterface_ReturnsImplementingComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing = childObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenInterfaceTarget>();
                var container = new Container();
                container.Bind<IInterface>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing = childObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenInterfaceTarget>();
                var container = new Container();
                container.Bind<IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenBindingIsNonGeneric_ReturnsImplementingComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing = childObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenInterfaceTarget>();
                var container = new Container();
                container.Bind(typeof(IInterface)).FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenNonGenericInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing = childObject.AddComponent<ScriptImplementedIInterface>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenInterfaceTarget>();
                var container = new Container();
                container.Bind(typeof(IInterface))
                    .To(typeof(ScriptImplementedIInterface))
                    .FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Service, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenContractIsAbstractComponent_ReturnsDerivedComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var existing =
                    childObject.AddComponent<ComponentInChildrenConcreteAbstractScript>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenAbstractTarget>();
                var container = new Container();
                container.Bind<AbstractScript>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenConsumerHasComponent_PrefersItOverChildren()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var expected = consumerObject.AddComponent<Script>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenOnlyGrandchildHasComponent_ReturnsIt()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");
            var grandchildObject = new GameObject("Grandchild");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                grandchildObject.transform.SetParent(childObject.transform);
                var expected = grandchildObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_UsesPreorderDepthFirstSearchBySiblingIndex()
        {
            var consumerObject = new GameObject("Consumer");
            var secondBranch = new GameObject("SecondBranch");
            var firstBranch = new GameObject("FirstBranch");
            var firstGrandchild = new GameObject("FirstGrandchild");

            try
            {
                secondBranch.transform.SetParent(consumerObject.transform);
                firstBranch.transform.SetParent(consumerObject.transform);
                firstGrandchild.transform.SetParent(firstBranch.transform);
                firstBranch.transform.SetSiblingIndex(0);
                secondBranch.transform.SetSiblingIndex(1);

                var expected = firstGrandchild.AddComponent<Script>();
                secondBranch.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenDescendantIsInactive_ReturnsItsComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var inactiveBranch = new GameObject("InactiveBranch");
            var targetObject = new GameObject("Target");

            try
            {
                inactiveBranch.transform.SetParent(consumerObject.transform);
                targetObject.transform.SetParent(inactiveBranch.transform);
                var expected = targetObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                inactiveBranch.SetActive(false);
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_CrossesLogicalGameObjectContextBoundary()
        {
            var parentContextObject = new GameObject("ParentContext");
            var consumerObject = new GameObject("Consumer");
            var childContextObject = new GameObject("ChildContext");
            var targetObject = new GameObject("Target");

            try
            {
                consumerObject.transform.SetParent(parentContextObject.transform);
                childContextObject.transform.SetParent(consumerObject.transform);
                targetObject.transform.SetParent(childContextObject.transform);

                var childContext = childContextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(childContext);
                var parentContext = parentContextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(
                    parentContext,
                    gameObjectContexts: new[] { childContext });
                parentContext.Initialize();

                var expected = targetObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                parentContext.Container.Bind<Script>().FromComponentInChildren();

                parentContext.Container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentContextObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_DoesNotSearchParentsOrSiblings()
        {
            var parentObject = new GameObject("Parent");
            var consumerObject = new GameObject("Consumer");
            var siblingObject = new GameObject("Sibling");

            try
            {
                consumerObject.transform.SetParent(parentObject.transform);
                siblingObject.transform.SetParent(parentObject.transform);
                parentObject.AddComponent<Script>();
                siblingObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

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
        public void Inject_FromComponentInChildren_WhenFirstObjectHasSeveralComponents_ReturnsFirst()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var expected = childObject.AddComponent<Script>();
                childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenComponentIsMissing_ReportsTypeAndConsumer()
        {
            var consumerObject = new GameObject("Consumer");

            try
            {
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                var exception = Assert.Throws<InvalidOperationException>(
                    () => container.Inject(consumer));

                Assert.That(exception.Message, Does.Contain(typeof(Script).ToString()));
                Assert.That(exception.Message, Does.Contain(consumerObject.name));
                Assert.That(
                    exception.Message,
                    Does.Contain(typeof(ComponentInChildrenScriptTarget).ToString()));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInChildren_WithoutConsumer_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInChildren();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Instantiate_FromComponentInChildren_WithoutConsumerInstance_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInChildren();

            Assert.That(
                () => container.Instantiate<ConstructorConsumer>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenConsumerIsNotMonoBehaviour_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInChildren();

            Assert.That(
                () => container.Inject(new PlainConsumer()),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenConsumerIsDestroyed_ThrowsInvalidOperationException()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren();

                UnityEngine.Object.DestroyImmediate(consumer);

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_AsTransient_RepeatsSearchForSameConsumer()
        {
            var consumerObject = new GameObject("Consumer");
            var firstBranch = new GameObject("FirstBranch");
            var secondBranch = new GameObject("SecondBranch");

            try
            {
                firstBranch.transform.SetParent(consumerObject.transform);
                secondBranch.transform.SetParent(consumerObject.transform);
                firstBranch.transform.SetSiblingIndex(0);
                secondBranch.transform.SetSiblingIndex(1);
                var firstComponent = firstBranch.AddComponent<Script>();
                var secondComponent = secondBranch.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren().AsTransient();

                container.Inject(consumer);
                var firstResolved = consumer.Script;
                firstBranch.transform.SetParent(null);
                container.Inject(consumer);

                Assert.That(firstResolved, Is.SameAs(firstComponent));
                Assert.That(consumer.Script, Is.SameAs(secondComponent));
            }
            finally
            {
                DestroyGameObjects(consumerObject, firstBranch, secondBranch);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_AsCached_UsesFirstSuccessfulConsumerComponent()
        {
            var firstConsumerObject = new GameObject("FirstConsumer");
            var firstChildObject = new GameObject("FirstChild");
            var secondConsumerObject = new GameObject("SecondConsumer");
            var secondChildObject = new GameObject("SecondChild");

            try
            {
                firstChildObject.transform.SetParent(firstConsumerObject.transform);
                secondChildObject.transform.SetParent(secondConsumerObject.transform);
                var firstComponent = firstChildObject.AddComponent<Script>();
                var secondComponent = secondChildObject.AddComponent<Script>();
                var firstConsumer =
                    firstConsumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var secondConsumer =
                    secondConsumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren().AsCached();

                container.Inject(firstConsumer);
                container.Inject(secondConsumer);

                Assert.That(firstConsumer.Script, Is.SameAs(firstComponent));
                Assert.That(secondConsumer.Script, Is.SameAs(firstComponent));
                Assert.That(secondConsumer.Script, Is.Not.SameAs(secondComponent));
            }
            finally
            {
                DestroyGameObjects(firstConsumerObject, secondConsumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_AsCached_DoesNotCacheFailedSearch()
        {
            var consumerObject = new GameObject("Consumer");

            try
            {
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var container = new Container();
                container.Bind<Script>().FromComponentInChildren().AsCached();

                Assert.That(
                    () => container.Inject(consumer),
                    Throws.TypeOf<InvalidOperationException>());

                var childObject = new GameObject("Child");
                childObject.transform.SetParent(consumerObject.transform);
                var expected = childObject.AddComponent<Script>();
                container.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Inject_FromComponentInChildren_WhenBindingComesFromParentContainer_UsesOriginalConsumer()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var expected = childObject.AddComponent<Script>();
                var consumer = consumerObject.AddComponent<ComponentInChildrenScriptTarget>();
                var bindingOwner = new Container();
                bindingOwner.Bind<Script>().FromComponentInChildren();
                var injectingContainer = new Container(bindingOwner);

                injectingContainer.Inject(consumer);

                Assert.That(consumer.Script, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(consumerObject);
            }
        }

        [Test]
        public void Build_FromComponentInChildren_NonLazy_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInChildren().AsTransient().NonLazy();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Build_FromComponentInChildren_AsEntryPoint_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<ScriptWithEntryPoint>().FromComponentInChildren().AsEntryPoint();

            Assert.That(
                () => container.Build(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_FromComponentInChildren_DoesNotInjectFoundComponent()
        {
            var consumerObject = new GameObject("Consumer");
            var childObject = new GameObject("Child");

            try
            {
                childObject.transform.SetParent(consumerObject.transform);
                var probe = childObject.AddComponent<ComponentInChildrenInjectionProbe>();
                var consumer =
                    consumerObject.AddComponent<ComponentInChildrenInjectionProbeTarget>();
                var container = new Container();
                container.Bind<ComponentInChildrenInjectionProbe>()
                    .FromComponentInChildren();

                container.Inject(consumer);

                Assert.That(consumer.Probe, Is.SameAs(probe));
                Assert.That(probe.InjectionCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(consumerObject);
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
