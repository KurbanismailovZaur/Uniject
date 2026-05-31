using Uniject.Getters;

namespace Uniject.Bindings
{
    public class BindingAsBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingAsBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingNonLazyBuilder AsTransient()
        {
            _binding.As(Scope.Transient);
            return new BindingNonLazyBuilder(_container, _binding);
        }

        public BindingNonLazyBuilder AsCached()
        {
            _binding.As(Scope.Cached);
            return new BindingNonLazyBuilder(_container, _binding);
        }
    }
}