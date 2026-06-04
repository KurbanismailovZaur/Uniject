using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;

namespace Uniject.Tests
{
    public class ContainerTests 
    {
        [Test]
        public void FromConstructor()
        {
            // Container container = new();
            
            // container.Bind<Concrete>().To<Concrete>().FromConstructor().AsTransient();
            // container.Bind<Concrete>().To<Concrete>().FromConstructor().AsTransient().NonLazy();
            // container.Bind<Concrete>().To<Concrete>().FromConstructor().AsCached();
            // container.Bind<Concrete>().To<Concrete>().FromConstructor().AsCached().NonLazy();
        }
    }
}