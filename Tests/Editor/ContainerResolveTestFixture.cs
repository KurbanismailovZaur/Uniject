using Uniject.Attributes;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public abstract class ContainerResolveTestFixture
    {
        protected class ParentDependencyInjectableClass
        {
            public Class Dependency { get; private set; }

            [Inject]
            public void Construct(Class dependency)
            {
                Dependency = dependency;
            }
        }
    }
}