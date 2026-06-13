using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class ClassWithEntryPoint : IEntryPoint
    {
        public void Run()
        {
            Debug.Log("Class Started!");
        }
    }
}
