using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveParentTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_WhenBindingExistsInParentContainer_ReturnsParentBindingInstance()
        {
            var dependency = new Class();

            var parent = new Container();
            parent.Bind<Class>().FromInstance(dependency);

            var child = new Container(parent);

            var resolved = child.Resolve<Class>();

            Assert.That(resolved, Is.SameAs(dependency));
        }

        [Test]
        public void Resolve_WhenBindingExistsInChildAndParent_ReturnsChildBindingInstance()
        {
            var parentDependency = new Class();
            var childDependency = new Class();

            var parent = new Container();
            parent.Bind<Class>().FromInstance(parentDependency);

            var child = new Container(parent);
            child.Bind<Class>().FromInstance(childDependency);

            var resolved = child.Resolve<Class>();

            Assert.That(resolved, Is.SameAs(childDependency));
        }

        [Test]
        public void Resolve_WhenBindingExistsInGrandParentContainer_ReturnsGrandParentBindingInstance()
        {
            var dependency = new Class();

            var grandParent = new Container();
            grandParent.Bind<Class>().FromInstance(dependency);

            var parent = new Container(grandParent);
            var child = new Container(parent);

            var resolved = child.Resolve<Class>();

            Assert.That(resolved, Is.SameAs(dependency));
        }

        [Test]
        public void Inject_WhenDependencyExistsInParentContainer_InjectsParentDependency()
        {
            var dependency = new Class();
            var target = new ParentDependencyInjectableClass();

            var parent = new Container();
            parent.Bind<Class>().FromInstance(dependency);

            var child = new Container(parent);
            child.Inject(target);

            Assert.That(target.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Resolve_WhenResolvingContainerFromChild_ReturnsChildContainer()
        {
            var parent = new Container();
            var child = new Container(parent);

            var resolved = child.Resolve<Container>();

            Assert.That(resolved, Is.SameAs(child));
        }

        [Test]
        public void Resolve_WhenBindingDoesNotExistInChildOrParent_ThrowsException()
        {
            var parent = new Container();
            var child = new Container(parent);

            Assert.That(() => child.Resolve<Class>(), Throws.Exception.With.Message.Contains("No binding found"));
        }
    }
}
