using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ReflectionCacheConstructorTests : ReflectionCacheTestFixture
    {
        [Test]
        public void GetConstructorInjectionData_WhenInjectConstructorExists_ReturnsInjectConstructor()
        {
            var data = ReflectionCache.GetConstructorInjectionData(typeof(TypeWithInjectConstructor));

            Assert.That(data.constructorInfo.IsDefined(typeof(InjectAttribute), false), Is.True);
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetConstructorInjectionData_WhenNoInjectConstructor_ReturnsConstructorWithMostParameters()
        {
            var data = ReflectionCache.GetConstructorInjectionData(typeof(TypeWithLongestConstructor));

            Assert.That(data.parametersInfo.Length, Is.EqualTo(2));
        }

        [Test]
        public void GetConstructorInjectionData_WhenMultipleInjectConstructors_ThrowsInvalidOperationException()
        {
            Assert.That(
                () => ReflectionCache.GetConstructorInjectionData(typeof(TypeWithMultipleInjectConstructors)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GetConstructorInjectionData_WhenNoPublicConstructor_ThrowsInvalidOperationException()
        {
            Assert.That(
                () => ReflectionCache.GetConstructorInjectionData(typeof(TypeWithPrivateConstructor)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GetConstructorInjectionData_WhenTypeHasOnlyParameterlessConstructor_ReturnsConstructor()
        {
            var data = ReflectionCache.GetConstructorInjectionData(typeof(TypeWithOnlyParameterlessConstructor));

            Assert.That(data.constructorInfo, Is.Not.Null);
            Assert.That(data.parametersInfo, Is.Empty);
        }
    }
}
