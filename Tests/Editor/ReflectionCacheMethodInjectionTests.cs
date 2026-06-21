using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ReflectionCacheMethodInjectionTests : ReflectionCacheTestFixture
    {
        [Test]
        public void GetMethodInjectionData_WhenInjectMethodExists_ReturnsInjectMethodData()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(TypeWithInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.Name, Is.EqualTo(nameof(TypeWithInjectMethod.Construct)));
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetMethodInjectionData_WhenNoInjectMethod_ReturnsDataWithoutInjectMethod()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(TypeWithoutInjectMethod));

            Assert.That(data.hasInjectMethod, Is.False);
            Assert.That(data.methodInfo, Is.Null);
            Assert.That(data.parametersInfo, Is.Empty);
        }

        [Test]
        public void GetMethodInjectionData_WhenMultipleInjectMethods_ThrowsInvalidOperationException()
        {
            Assert.That(
                () => ReflectionCache.GetMethodInjectionData(typeof(TypeWithMultipleInjectMethods)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GetMethodInjectionData_WhenPrivateInjectMethodExists_ReturnsInjectMethodData()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(TypeWithPrivateInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.Name, Is.EqualTo("Construct"));
            Assert.That(data.methodInfo.IsPrivate, Is.True);
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
            Assert.That(data.parametersInfo[0].ParameterType, Is.EqualTo(typeof(DependencyA)));
        }

        [Test]
        public void GetMethodInjectionData_WhenProtectedInjectMethodExists_ReturnsInjectMethodData()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(TypeWithProtectedInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.Name, Is.EqualTo("Construct"));
            Assert.That(data.methodInfo.IsFamily, Is.True);
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
            Assert.That(data.parametersInfo[0].ParameterType, Is.EqualTo(typeof(DependencyA)));
        }

        [Test]
        public void GetMethodInjectionData_WhenBaseAndDerivedHaveInjectMethods_ReturnsOnlyDerivedMethod()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(DerivedTypeWithOwnInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.DeclaringType, Is.EqualTo(typeof(DerivedTypeWithOwnInjectMethod)));
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
            Assert.That(data.parametersInfo[0].ParameterType, Is.EqualTo(typeof(DependencyB)));
        }

        [Test]
        public void GetMethodInjectionData_WhenOnlyBaseHasInjectMethod_ReturnsBaseMethod()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(DerivedTypeWithoutOwnInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.DeclaringType, Is.EqualTo(typeof(BaseTypeWithInjectMethod)));
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
            Assert.That(data.parametersInfo[0].ParameterType, Is.EqualTo(typeof(DependencyA)));
        }

        [Test]
        public void GetMethodInjectionData_WhenGrandBaseHasInjectMethod_ReturnsGrandBaseMethod()
        {
            var data = ReflectionCache.GetMethodInjectionData(typeof(DerivedFromMiddleTypeWithoutInjectMethod));

            Assert.That(data.hasInjectMethod, Is.True);
            Assert.That(data.methodInfo.DeclaringType, Is.EqualTo(typeof(BaseTypeWithInjectMethod)));
            Assert.That(data.parametersInfo.Length, Is.EqualTo(1));
            Assert.That(data.parametersInfo[0].ParameterType, Is.EqualTo(typeof(DependencyA)));
        }
    }
}
