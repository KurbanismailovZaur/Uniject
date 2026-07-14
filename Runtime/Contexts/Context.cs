using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] protected List<MonoInstaller> _installers;
        [SerializeField] protected List<MonoBehaviour> _injectTargets;
        [SerializeField] protected List<GameObjectContext> _gameObjectContexts;
        [SerializeField] protected Transform ParentTransformForGameObjects;
        
        public Container Container { get; protected set; }
        public bool IsInitialized { get; protected set; }
        public bool IsInstalled { get; protected set; }
        public bool IsBuilded { get; protected set; }

        public virtual void Initialize(Container parentContainer = null)
        {
            if (IsInitialized)
                return;
            
            IsInitialized = true;
            Container = new Container (parentContainer, ParentTransformForGameObjects); 

            foreach (var context in _gameObjectContexts)
                context.Initialize(Container);
        }

        public virtual void Install()
        {
            if (IsInstalled)
                return;

            IsInstalled = true;

            Container.BindInstance(this);
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

            foreach (var context in _gameObjectContexts)
                context.Install();
        }

        public virtual void Build()
        {
            if (IsBuilded)
                return;

            IsBuilded = true;

            Container.Build();

            foreach (var context in _gameObjectContexts)
                context.Build();
        }

        public void Run()
        {
            Initialize();
            Install();
            Build();
        }

        protected virtual void OnDestroy()
        {
            if (IsBuilded)
                Container.Dispose();
        }
    }
}
