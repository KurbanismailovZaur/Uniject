using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;

namespace Uniject.Tests
{
    public class ReflectionCacheTests
    {
        private sealed class DependencyA { }
        private sealed class DependencyB { }

        private sealed class TypeWithInjectConstructor
        {
            public TypeWithInjectConstructor(DependencyA a, DependencyB b) { }

            [Inject]
            public TypeWithInjectConstructor(DependencyA a) { }
        }

        private sealed class TypeWithLongestConstructor
        {
            public TypeWithLongestConstructor() { }
            public TypeWithLongestConstructor(DependencyA a) { }
            public TypeWithLongestConstructor(DependencyA a, DependencyB b) { }
        }

        private sealed class TypeWithMultipleInjectConstructors
        {
            [Inject]
            public TypeWithMultipleInjectConstructors(DependencyA a) { }

            [Inject]
            public TypeWithMultipleInjectConstructors(DependencyB b) { }
        }

        private sealed class TypeWithPrivateConstructor
        {
            private TypeWithPrivateConstructor() { }
        }

        private sealed class TypeWithInjectMethod
        {
            [Inject]
            public void Construct(DependencyA a) { }
        }

        private sealed class TypeWithoutInjectMethod
        {
            public void Construct(DependencyA a) { }
        }

        private sealed class TypeWithMultipleInjectMethods
        {
            [Inject]
            public void Construct(DependencyA a) { }

            [Inject]
            public void Initialize(DependencyB b) { }
        }

        private sealed class TypeWithOnlyParameterlessConstructor { }

        private class TypeWithPrivateInjectMethod
        {
            [Inject]
            private void Construct(DependencyA a) { }
        }

        private class TypeWithProtectedInjectMethod
        {
            [Inject]
            protected void Construct(DependencyA a) { }
        }

        private class BaseTypeWithInjectMethod
        {
            [Inject]
            private void Construct(DependencyA a) { }
        }

        private class DerivedTypeWithOwnInjectMethod : BaseTypeWithInjectMethod
        {
            [Inject]
            private void Construct(DependencyB b) { }
        }

        private class DerivedTypeWithoutOwnInjectMethod : BaseTypeWithInjectMethod
        {
        }

        private class MiddleTypeWithoutInjectMethod : BaseTypeWithInjectMethod
        {
        }

        private class DerivedFromMiddleTypeWithoutInjectMethod : MiddleTypeWithoutInjectMethod
        {
        }

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
        public void GetConstructorInjectionData_WhenTypeHasOnlyParameterlessConstructor_ReturnsConstructor()
        {
            var data = ReflectionCache.GetConstructorInjectionData(typeof(TypeWithOnlyParameterlessConstructor));

            Assert.That(data.constructorInfo, Is.Not.Null);
            Assert.That(data.parametersInfo, Is.Empty);
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
