using System;
using Uniject.InstanceGetters;
using Uniject.SubcontainerGetters;

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
            if (installMethod == null)
                throw new ArgumentNullException(nameof(installMethod), "Install method can not be a null.");

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
    }
}
