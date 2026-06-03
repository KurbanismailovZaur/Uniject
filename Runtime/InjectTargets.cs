using UnityEngine;

namespace Uniject
{
    public class InjectTargets : MonoBehaviour
    {
        [field: SerializeField] public MonoBehaviour[] Targets { get; private set; }
    }
}