using System;
using Uniject.Installers;
using Uniject.InstanceGetters;
using Uniject.SubcontainerGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeByBuilder : BindingToTypeBuilder
    {
        public BindingToTypeByBuilder(Container container, BindingToType binding) : base(container, binding) { }

        private InstanceGetterFromSubContainerResolve SetSubcontainerGetter(SubcontainerGetter subcontainerGetter)
        {
            var instanceGetter = (InstanceGetterFromSubContainerResolve)_binding.InstanceGetter;
            instanceGetter.SubcontainerGetter = subcontainerGetter;
            return instanceGetter;
        }

        public BindingToSubcontainerAsBuilder ByInstance(Container instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Subcontainer instance can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByInstance(_container, instance));
            return new(_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerAsBuilder ByMethod(Action<Container> installMethod)
        {
            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByMethod(_container, installMethod));
            return new (_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerAsBuilder ByInstaller<TInstaller>() where TInstaller : IInstaller, new()
        {
            var installer = _container.Instantiate<TInstaller>(typeof(TInstaller));
            return ByInstaller(installer);
        }

        public BindingToSubcontainerAsBuilder ByInstaller<TInstaller>(TInstaller installer) where TInstaller : IInstaller, new()
        {
            if (installer == null)
                throw new ArgumentNullException(nameof(installer), "Installer can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByInstaller(_container, installer));
            return new (_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromMethodOnNewGameObject(Action<Container> installMethod)
        {
            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByNewContextFromMethodOnNewGameObject(_container, installMethod));
            return new (_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromInstallerOnNewGameObject<TInstaller>() 
            where TInstaller : IInstaller, new()
        {
            var installer = _container.Instantiate<TInstaller>(typeof(TInstaller));
            return ByNewContextFromInstallerOnNewGameObject(installer);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromInstallerOnNewGameObject<TInstaller>(TInstaller installer) 
            where TInstaller : IInstaller, new()
        {
            if (installer == null)
                throw new ArgumentNullException(nameof(installer), "Installer can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByNewContextFromInstallerOnNewGameObject(_container, installer));
            return new (_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromMethodOnNewPrefab(GameObject prefab, Action<Container> installMethod)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByNewContextFromMethodOnNewPrefab(_container, prefab, installMethod));
            return new (_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromInstallerOnNewPrefab<TInstaller>(GameObject prefab)
            where TInstaller : IInstaller, new()
        {
            var installer = _container.Instantiate<TInstaller>(typeof(TInstaller));
            return ByNewContextFromInstallerOnNewPrefab(prefab, installer);
        }

        public BindingToSubcontainerWithGameObjectNameBuilder ByNewContextFromInstallerOnNewPrefab<TInstaller>(GameObject prefab, TInstaller installer)
            where TInstaller : IInstaller, new()
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab can not be a null.");

            if (installer == null)
                throw new ArgumentNullException(nameof(installer), "Installer can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByNewContextFromInstallerOnNewPrefab(_container, prefab, installer));
            return new (_container, _binding, instanceGetter);
        }
    
        public BindingToSubcontainerWithGameObjectNameBuilder ByContextOnNewPrefab(GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab can not be a null.");

            var instanceGetter = SetSubcontainerGetter(new SubcontainerGetterByContextOnNewPrefab(_container, prefab));
            return new (_container, _binding, instanceGetter);
        }
    }
}
