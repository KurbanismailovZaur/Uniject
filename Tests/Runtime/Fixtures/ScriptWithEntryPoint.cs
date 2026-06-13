using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class ScriptWithEntryPoint : MonoBehaviour, IEntryPoint
    {
        void IEntryPoint.Start()
        {
            Debug.Log("Started!");
        }
    }
}
