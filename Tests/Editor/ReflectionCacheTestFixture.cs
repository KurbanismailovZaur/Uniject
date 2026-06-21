using Uniject.Attributes;

namespace Uniject.Tests
{
    public abstract class ReflectionCacheTestFixture
    {
        protected sealed class DependencyA { }
        protected sealed class DependencyB { }

        protected sealed class TypeWithInjectConstructor
        {
            public TypeWithInjectConstructor(DependencyA a, DependencyB b) { }

            [Inject]
            public TypeWithInjectConstructor(DependencyA a) { }
        }

        protected sealed class TypeWithLongestConstructor
        {
            public TypeWithLongestConstructor() { }
            public TypeWithLongestConstructor(DependencyA a) { }
            public TypeWithLongestConstructor(DependencyA a, DependencyB b) { }
        }

        protected sealed class TypeWithMultipleInjectConstructors
        {
            [Inject]
            public TypeWithMultipleInjectConstructors(DependencyA a) { }

            [Inject]
            public TypeWithMultipleInjectConstructors(DependencyB b) { }
        }

        protected sealed class TypeWithPrivateConstructor
        {
            private TypeWithPrivateConstructor() { }
        }

        protected sealed class TypeWithInjectMethod
        {
            [Inject]
            public void Construct(DependencyA a) { }
        }

        protected sealed class TypeWithoutInjectMethod
        {
            public void Construct(DependencyA a) { }
        }

        protected sealed class TypeWithMultipleInjectMethods
        {
            [Inject]
            public void Construct(DependencyA a) { }

            [Inject]
            public void Initialize(DependencyB b) { }
        }

        protected sealed class TypeWithOnlyParameterlessConstructor { }

        protected class TypeWithPrivateInjectMethod
        {
            [Inject]
            private void Construct(DependencyA a) { }
        }

        protected class TypeWithProtectedInjectMethod
        {
            [Inject]
            protected void Construct(DependencyA a) { }
        }

        protected class BaseTypeWithInjectMethod
        {
            [Inject]
            private void Construct(DependencyA a) { }
        }

        protected class DerivedTypeWithOwnInjectMethod : BaseTypeWithInjectMethod
        {
            [Inject]
            private void Construct(DependencyB b) { }
        }

        protected class DerivedTypeWithoutOwnInjectMethod : BaseTypeWithInjectMethod
        {
        }

        protected class MiddleTypeWithoutInjectMethod : BaseTypeWithInjectMethod
        {
        }

        protected class DerivedFromMiddleTypeWithoutInjectMethod : MiddleTypeWithoutInjectMethod
        {
        }
    }
}