using System;
using Uniject.Bindings.Pools;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;

namespace Uniject
{
    public abstract class Pool
    {
        protected int _initialSize;
        protected int _maxSize;
        protected ExpandType _expandType;
    }

    public class Pool<TResult> : Pool, IPool<TResult>
    {
        protected Factory<TResult> _factory;

        internal void Construct(InstanceGetter instanceGetter, Type resultConcreteType, int initialSize, int maxSize, ExpandType expandType)
        {
            _initialSize = initialSize;
            _maxSize = maxSize;
            _expandType = expandType;

            var factory = new Factory<TResult>();
            factory.Construct(instanceGetter, resultConcreteType);
            _factory = factory;
        }

        public TResult Spawn() => _factory.Create();

        public void Despawn(TResult instance)
        {
            if (instance is UnityEngine.Object unityObject)
                UnityEngine.Object.Destroy(unityObject);
        }
    }

    public class Pool<TParam, TResult> : Pool, IPool<TParam, TResult>
    {
        protected Factory<TParam, TResult> _factory;

        internal void Construct(InstanceGetterWithParameter<TParam> instanceGetter, Type resultConcreteType, int initialSize, int maxSize, ExpandType expandType)
        {
            _initialSize = initialSize;
            _maxSize = maxSize;
            _expandType = expandType;

            var factory = new Factory<TParam, TResult>();
            factory.Construct(instanceGetter, resultConcreteType);
            _factory = factory;
        }

        public TResult Spawn(TParam origin) => _factory.Create(origin);

        public void Despawn(TResult instance)
        {
            if (instance is UnityEngine.Object unityObject)
                UnityEngine.Object.Destroy(unityObject);
        }
    }
}