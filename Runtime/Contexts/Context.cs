using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] private List<MonoInstaller> _installers;
        [SerializeField] private List<MonoBehaviour> _injectTargets;
        [SerializeField] private List<GameObjectContext> _gameObjectContexts;
        
        public Container Container { get; protected set; }
        public bool IsBuilded => Container?.IsBuilded ?? false;

        public virtual void Build()
        {
            if (IsBuilded)
                return;
            
            Container.BindInstance(this);
            Container.Bind<SceneLoader>().AsCached();
            Container.BindInstance(gameObject.AddComponent<TickableManager>());

            foreach (var installer in _installers)
                installer.Install(Container);

            foreach (var target in _injectTargets)
            {
                if (target is InjectTargets injectTargets)
                    Container.AddToInjectionQueue(injectTargets.Targets);
                else
                    Container.AddToInjectionQueue(target);
            }

            Container.Build();
        }

        protected virtual void OnDestroy()
        {
            if (IsBuilded)
                Container.Dispose();
        }
    }
}
