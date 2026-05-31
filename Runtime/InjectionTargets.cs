using UnityEngine;

namespace Uniject
{
    public class InjectionTargets : MonoBehaviour
    {
        [field: SerializeField] public MonoBehaviour[] Targets { get; private set; }
    }
}