using System;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;

namespace Uniject
{
    public interface IProvider<T>
    {
        bool HasData { get; }

        T Data { get; }
    }
}