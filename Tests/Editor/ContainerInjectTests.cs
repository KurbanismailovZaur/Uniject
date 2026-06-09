using System.Collections.Generic;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Exceptions;
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
        public void Inject_WhenInjectMethodHasNoParameters_ThrowsInjectException()
        {
            var container = new Container();
            var target = new ParameterlessInjectableClass();

            Assert.That(() => container.Inject(target), Throws.TypeOf<InjectException>());
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
        public void Inject_WhenInstanceHasMultipleInjectMethods_ThrowsInjectException()
        {
            var container = new Container();
            var target = new ClassWithMultipleInjectMethods();

            Assert.That(
                () => container.Inject(target),
                Throws.TypeOf<InjectException>());
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
    }
}
