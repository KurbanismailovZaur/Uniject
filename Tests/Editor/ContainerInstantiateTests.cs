using System;
using NUnit.Framework;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerInstantiateTests
    {
        private class ClassWithConstructorDependency
        {
            public Class Dependency { get; }

            public ClassWithConstructorDependency(Class dependency)
            {
                Dependency = dependency;
            }
        }

        private class ClassWithMultipleConstructorDependencies
        {
            public Class ClassDependency { get; }
            public IInterface InterfaceDependency { get; }

            public ClassWithMultipleConstructorDependencies(Class classDependency, IInterface interfaceDependency)
            {
                ClassDependency = classDependency;
                InterfaceDependency = interfaceDependency;
            }
        }

        [Test]
        public void Instantiate_WhenTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Instantiate((Type)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Instantiate_WhenTypeHasParameterlessConstructor_ReturnsInstance()
        {
            var container = new Container();

            var instance = container.Instantiate<Class>();

            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void Instantiate_WhenTypeHasConstructorDependency_ResolvesDependency()
        {
            var container = new Container();
            var dependency = new Class();

            container.Bind<Class>().FromInstance(dependency);

            var instance = container.Instantiate<ClassWithConstructorDependency>();

            Assert.That(instance.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Instantiate_WhenTypeHasMultipleConstructorDependencies_ResolvesAllDependencies()
        {
            var container = new Container();
            var classDependency = new Class();
            var interfaceDependency = new ClassImplementedIInterface();

            container.Bind<Class>().FromInstance(classDependency);
            container.Bind<IInterface>().FromInstance(interfaceDependency);

            var instance = (ClassWithMultipleConstructorDependencies)container.Instantiate(typeof(ClassWithMultipleConstructorDependencies));

            Assert.That(instance.ClassDependency, Is.SameAs(classDependency));
            Assert.That(instance.InterfaceDependency, Is.SameAs(interfaceDependency));
        }

        [Test]
        public void Instantiate_WhenGenericContractAndConcreteTypeArePassed_ReturnsConcreteInstance()
        {
            var container = new Container();

            var instance = container.Instantiate<IInterface>(typeof(ClassImplementedIInterface));

            Assert.That(instance, Is.InstanceOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Instantiate_WhenConstructorDependencyIsNotBound_ThrowsException()
        {
            var container = new Container();

            Assert.That(
                () => container.Instantiate<ClassWithConstructorDependency>(),
                Throws.Exception.With.Message.Contains("No binding found"));
        }

        [Test]
        public void Instantiate_WhenPrefabIsGameObject_ReturnsClonedGameObject()
        {
            var prefab = new GameObject("Prefab");
            var cloned = default(GameObject);

            try
            {
                prefab.AddComponent<Script>();

                var container = new Container();
                cloned = container.Instantiate(prefab);

                Assert.That(cloned, Is.Not.Null);
                Assert.That(cloned, Is.Not.SameAs(prefab));
                Assert.That(cloned.GetComponent<Script>(), Is.Not.Null);
                Assert.That(cloned.GetComponent<Script>(), Is.Not.SameAs(prefab.GetComponent<Script>()));
            }
            finally
            {
                if (cloned != null)
                    UnityEngine.Object.DestroyImmediate(cloned);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Instantiate_WhenPrefabIsComponent_ReturnsClonedComponent()
        {
            var prefabGameObject = new GameObject("Prefab");
            var clonedScript = default(Script);

            try
            {
                var prefabScript = prefabGameObject.AddComponent<Script>();

                var container = new Container();
                clonedScript = container.Instantiate(prefabScript);

                Assert.That(clonedScript, Is.Not.Null);
                Assert.That(clonedScript, Is.Not.SameAs(prefabScript));
                Assert.That(clonedScript.gameObject, Is.Not.SameAs(prefabGameObject));
            }
            finally
            {
                if (clonedScript != null)
                    UnityEngine.Object.DestroyImmediate(clonedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabGameObject);
            }
        }
    }
}
