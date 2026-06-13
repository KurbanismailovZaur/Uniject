using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public class ContainerInjectTests
    {
        private class InjectableClass
        {
            public Class Dependency { get; private set; }
            public int CallsCount { get; private set; }

            [Inject]
            public void Construct(Class dependency)
            {
                Dependency = dependency;
                CallsCount++;
            }
        }

        private class ParameterlessInjectableClass
        {
            public bool WasInjected { get; private set; }

            [Inject]
            public void Construct() => WasInjected = true;
        }

        private class MultiDependencyInjectableClass
        {
            public Class ClassDependency { get; private set; }
            public IInterface InterfaceDependency { get; private set; }

            [Inject]
            public void Construct(Class classDependency, IInterface interfaceDependency)
            {
                ClassDependency = classDependency;
                InterfaceDependency = interfaceDependency;
            }
        }

        private class ClassWithoutInjectMethod
        {
            public bool WasConstructCalled { get; private set; }

            public void Construct(Class dependency) => WasConstructCalled = true;
        }

        private class ClassWithMultipleInjectMethods
        {
            [Inject]
            public void Construct(Class dependency) { }

            [Inject]
            public void Initialize(Class dependency) { }
        }

        private class OrderedInjectableClass
        {
            private readonly int _id;
            private readonly List<int> _calls;

            public OrderedInjectableClass(int id, List<int> calls)
            {
                _id = id;
                _calls = calls;
            }

            [Inject]
            public void Construct(Class dependency)
            {
                _calls.Add(_id);
            }
        }

        private static void InjectQueuedInstances(Container container)
        {
            var method = typeof(Container).GetMethod("InjectQueuedInstances", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(container, null);
        }

        [Test]
        public void Inject_WhenInstanceHasInjectMethod_ResolvesDependencyAndInvokesMethod()
        {
            var container = new Container();
            var dependency = new Class();
            var target = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.Inject(target);

            Assert.That(target.Dependency, Is.SameAs(dependency));
            Assert.That(target.CallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Inject_WhenInjectMethodHasNoParameters_ThrowsInvalidOperationException()
        {
            var container = new Container();
            var target = new ParameterlessInjectableClass();

            Assert.That(() => container.Inject(target), Throws.TypeOf<InvalidOperationException>());
            Assert.That(target.WasInjected, Is.False);
        }

        [Test]
        public void Inject_WhenInjectMethodHasMultipleParameters_ResolvesAllDependencies()
        {
            var container = new Container();
            var classDependency = new Class();
            var interfaceDependency = new ClassImplementedIInterface();
            var target = new MultiDependencyInjectableClass();

            container.Bind<Class>().FromInstance(classDependency);
            container.Bind<IInterface>().FromInstance(interfaceDependency);

            container.Inject(target);

            Assert.That(target.ClassDependency, Is.SameAs(classDependency));
            Assert.That(target.InterfaceDependency, Is.SameAs(interfaceDependency));
        }

        [Test]
        public void Inject_WhenInstanceHasNoInjectMethod_DoesNothing()
        {
            var container = new Container();
            var target = new ClassWithoutInjectMethod();

            Assert.That(() => container.Inject(target), Throws.Nothing);
            Assert.That(target.WasConstructCalled, Is.False);
        }

        [Test]
        public void Inject_WhenDependencyIsNotBound_ThrowsException()
        {
            var container = new Container();
            var target = new InjectableClass();

            Assert.That(
                () => container.Inject(target),
                Throws.Exception.With.Message.Contains("No binding found"));
        }

        [Test]
        public void Inject_WhenInstanceHasMultipleInjectMethods_ThrowsInvalidOperationException()
        {
            var container = new Container();
            var target = new ClassWithMultipleInjectMethods();

            Assert.That(
                () => container.Inject(target),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Inject_WhenEnumerableIsPassed_InjectsEveryInstance()
        {
            var container = new Container();
            var dependency = new Class();
            var first = new InjectableClass();
            var second = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.Inject((IEnumerable<object>)new object[] { first, second });

            Assert.That(first.Dependency, Is.SameAs(dependency));
            Assert.That(second.Dependency, Is.SameAs(dependency));
        }
        
        [Test]
        public void AddToInjectionQueue_WhenSameInstanceIsAddedTwice_ThrowsArgumentException()
        {
            var container = new Container();
            var target = new InjectableClass();

            container.AddToInjectionQueue(target);

            Assert.That(() => container.AddToInjectionQueue(target), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AddToInjectionQueue_WhenInstancesArrayContainsSameInstanceTwice_ThrowsArgumentException()
        {
            var container = new Container();
            var target = new InjectableClass();

            Assert.That(() => container.AddToInjectionQueue(target, target), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void InjectQueuedInstances_WhenSingleInstanceWasQueued_InjectsInstance()
        {
            var container = new Container();
            var dependency = new Class();
            var target = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.AddToInjectionQueue(target);

            InjectQueuedInstances(container);

            Assert.That(target.Dependency, Is.SameAs(dependency));
            Assert.That(target.CallsCount, Is.EqualTo(1));
        }

        [Test]
        public void InjectQueuedInstances_WhenMultipleInstancesWereQueued_InjectsEveryInstance()
        {
            var container = new Container();
            var dependency = new Class();
            var first = new InjectableClass();
            var second = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.AddToInjectionQueue(first, second);

            InjectQueuedInstances(container);

            Assert.That(first.Dependency, Is.SameAs(dependency));
            Assert.That(second.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void InjectQueuedInstances_WhenCalledTwice_DoesNotInjectAlreadyProcessedInstances()
        {
            var container = new Container();
            var dependency = new Class();
            var target = new InjectableClass();

            container.Bind<Class>().FromInstance(dependency);
            container.AddToInjectionQueue(target);

            InjectQueuedInstances(container);
            InjectQueuedInstances(container);

            Assert.That(target.CallsCount, Is.EqualTo(1));
        }

        [Test]
        public void InjectQueuedInstances_WhenQueueIsEmpty_DoesNothing()
        {
            var container = new Container();
            Assert.That(() => InjectQueuedInstances(container), Throws.Nothing);
        }

        [Test]
        public void AddToInjectionQueue_WhenInstanceIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            Assert.That(() => container.AddToInjectionQueue((object)null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddToInjectionQueue_WhenInstancesArrayIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            Assert.That(() => container.AddToInjectionQueue((object[])null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddToInjectionQueue_WhenAnyInstanceInArrayIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            Assert.That(() => container.AddToInjectionQueue(new InjectableClass(), null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void InjectQueuedInstances_InjectsInstancesInQueueOrder()
        {
            var container = new Container();
            var dependency = new Class();
            var calls = new List<int>();
            var first = new OrderedInjectableClass(1, calls);
            var second = new OrderedInjectableClass(2, calls);

            container.Bind<Class>().FromInstance(dependency);
            container.AddToInjectionQueue(first, second);

            InjectQueuedInstances(container);

            Assert.That(calls, Is.EqualTo(new[] { 1, 2 }));
        }
    }
}
