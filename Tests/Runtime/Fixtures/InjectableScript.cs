using Uniject.Attributes;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class InjectableScript : MonoBehaviour
    {
        public Class Dependency { get; private set; }

        [Inject]
        public void Construct(Class dependency)
        {
            Dependency = dependency;
        }
    }
}
