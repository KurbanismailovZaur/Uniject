using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromCustomFactoryTests : ContainerFactoryTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            InitializableCustomProductFactory.Reset();
        }

        [Test]
        public void Create_FromFactory_UsesCustomFactory()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().To<Product>().FromFactory<CustomProductFactory>().AsTransient();

            var product = container.Resolve<ProductFactory>().Create();

            Assert.That(product, Is.TypeOf<Product>());
        }

        [Test]
        public void FromFactory_InitializesCustomFactoryOnceBeforeCreate()
        {
            var dependency = new Class();
            var container = new Container();
            container.Bind<Class>().FromInstance(dependency);
            container.BindFactory<Product, ProductFactory>()
                .To<Product>()
                .FromFactory<InitializableCustomProductFactory>()
                .AsTransient();

            Assert.That(InitializableCustomProductFactory.InitializeCallsCount, Is.EqualTo(1));
            Assert.That(InitializableCustomProductFactory.CreateCallsCount, Is.Zero);
            Assert.That(InitializableCustomProductFactory.ResolvedDependency, Is.SameAs(dependency));

            var firstFactory = container.Resolve<ProductFactory>();
            var secondFactory = container.Resolve<ProductFactory>();
            firstFactory.Create();
            secondFactory.Create();

            Assert.That(InitializableCustomProductFactory.InitializeCallsCount, Is.EqualTo(1));
            Assert.That(InitializableCustomProductFactory.CreateCallsCount, Is.EqualTo(2));
            Assert.That(InitializableCustomProductFactory.WasInitializedBeforeCreate, Is.True);
        }
    }
}
