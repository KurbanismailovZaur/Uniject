using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class ClassWithEntryPoint : IEntryPoint
    {
        public void Start()
        {
            Debug.Log("Class Started!");
        }
    }
}
