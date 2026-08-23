using Uniject.Attributes;
using UnityEngine;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    [Preserve]
    public sealed class PerformanceUnityObjectInjectTarget : MonoBehaviour
    {
        private UnityObjectPerformanceTests.PerformanceUnityObjectDependency _dependency;

        [Inject, Preserve]
        private void Construct(UnityObjectPerformanceTests.PerformanceUnityObjectDependency dependency)
        {
            _dependency = dependency;
        }
    }
}
