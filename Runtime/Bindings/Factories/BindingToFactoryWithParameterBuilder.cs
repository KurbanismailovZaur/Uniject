using System;
using Uniject.Bindings;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameterBuilder<TParam, TResult, TFactory> 
        where TFactory : Factory<TParam, TResult>, new()
    {
        protected readonly BindingToFactoryWithParameter<TParam, TResult, TFactory> _binding;
        protected readonly Container _container;

        public BindingToFactoryWithParameterBuilder(Container container, BindingToFactoryWithParameter<TParam, TResult, TFactory> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}