using System;
using Uniject.InstanceGetters;
using Uniject.SubcontainerGetters;

namespace Uniject.Bindings
{
    public class BindingToTypeByBuilder : BindingToTypeBuilder
    {
        public BindingToTypeByBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToSubcontainerAsBuilder ByInstance(Container instance)
        {
            instance.SetParentContainer(_container);
            var instanceGetter = (InstanceGetterFromSubContainerResolve)_binding.InstanceGetter;
            instanceGetter.SubcontainerGetter = new SubcontainerGetterByInstance(_container, instance);
            return new(_container, _binding, instanceGetter);
        }

        public BindingToSubcontainerAsBuilder ByMethod(Action<Container> installMethod)
        {
            return default;
        }

        public BindingToSubcontainerAsBuilder ByInstaller<TInstaller>(TInstaller installer) where TInstaller : IInstaller
        {
            return default;
        }
    }
}
