using UnityEngine;

namespace Uniject.Installers
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(Container container);
    }
}