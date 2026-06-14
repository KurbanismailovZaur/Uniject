using UnityEngine;

namespace Uniject.Components
{
    public class InjectTargets : MonoBehaviour
    {
        [field: SerializeField] public MonoBehaviour[] Targets { get; private set; }
    }
}
