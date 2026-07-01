using System;
using Uniject.InstanceGetters;

namespace Uniject.Bindings.Pools
{
    public class BindingToPool<TResult, TPool> : Binding where TPool : Pool<TResult>, new()
    {
        public InstanceGetter InstanceGetter { get; set; }
        public Type ResultContractType { get; set; }
        public Type ResultConcreteType { get; set; }
        public int InitialSize { get; set; }
        public int MaxSize { get; set; }
        public ExpandType ExpandType { get; set; }

        public BindingToPool(Container container, Type resultType, Type poolType) : base(container, poolType)
        {
            InstanceGetter = new InstanceGetterFromConstructor(container);
            ResultContractType = resultType;
            ResultConcreteType = resultType;
        }

        private object CreatePool()
        {
            var pool = new TPool();
            pool.Construct(InstanceGetter, ResultConcreteType, InitialSize, MaxSize, ExpandType);
            return pool;
        }

        public override object GetInstance()
        {
            if (Scope == Scope.Transient)
                return CreatePool();

            return CachedInstance ??= CreatePool();
        }
    }
}