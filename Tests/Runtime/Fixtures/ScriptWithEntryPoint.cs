using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class ScriptWithEntryPoint : MonoBehaviour, IEntryPoint
    {
        public void Run()
        {
            Debug.Log("Script Started!");
        }
    }
}
