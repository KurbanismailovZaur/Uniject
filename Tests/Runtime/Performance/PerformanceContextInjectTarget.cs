using Uniject.Attributes;
using UnityEngine;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    [Preserve]
    public sealed class PerformanceContextInjectTarget : MonoBehaviour
    {
        private ContextPerformanceTests.PerformanceContextDependency _dependency;

        [Inject, Preserve]
        private void Construct(ContextPerformanceTests.PerformanceContextDependency dependency)
        {
            _dependency = dependency;
        }
    }
}
