using System;
using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Contexts
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] protected bool _useSiblingInstallers = true;
        [SerializeField] protected List<MonoInstaller> _installers = new();
        [SerializeField] protected bool _injectInAllContextGameObjects = true;
        [SerializeField] protected List<MonoBehaviour> _injectTargets = new();
        [SerializeField] protected List<GameObjectContext> _gameObjectContexts = new();
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
            Container = new Container(parentContainer, ParentTransformForGameObjects, this); 

            foreach (var context in _gameObjectContexts)
                context.Initialize(Container);
        }

        public virtual void Install()
        {
            if (IsInstalled)
                return;

            IsInstalled = true;

            var installers = _useSiblingInstallers ? (IList<MonoInstaller>)GetComponents<MonoInstaller>() : _installers;
            
            foreach (var installer in installers)
                installer.Install(Container);               

            if (_injectInAllContextGameObjects)
            {
                var injectTargets = StaticCollections.collectionPool.SpawnList<MonoBehaviour>();
                var monoBehaviours = StaticCollections.collectionPool.SpawnList<MonoBehaviour>();
                var roots = gameObject.scene.GetRootGameObjects();

                foreach (var root in roots)
                {
                    root.GetComponentsInChildren(includeInactive: true, monoBehaviours);
                    injectTargets.AddRange(monoBehaviours);
                }

                StaticCollections.collectionPool.DespawnList(monoBehaviours);

                foreach (var target in injectTargets)
                    Container.AddToInjectionQueue(target);

                StaticCollections.collectionPool.DespawnList(injectTargets);
            }
            else
            {
                foreach (var target in _injectTargets)
                {
                    if (target is InjectTargets injectTargetsComponent)
                        Container.AddToInjectionQueue(injectTargetsComponent.Targets);
                    else
                        Container.AddToInjectionQueue(target);
                }
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
