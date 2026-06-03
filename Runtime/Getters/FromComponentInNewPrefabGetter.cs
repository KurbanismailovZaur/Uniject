using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter<TConcrete> : InstanceGetter
    {
        private readonly TConcrete _prefab;

        public FromComponentInNewPrefabGetter(Container container, TConcrete prefab, Type concreteType) : base(container)
        {
            if (prefab == null) 
                throw new ArgumentNullException(nameof(prefab), 
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter can not be null.");

            
        }

        public override object GetObject(Type concreteType) => _container.Instantiate(_prefab as Component);

        // TODO: Короче надо чекать какой тип в To и приводить в _prefab присваивать этот экземпляр, а затем в
        // методе GetObject вызывать Instantiate(gameobject) и в зависимости от того что лежит в _prefab,
        // возвращать либо компонент, либо геймобджект. Также надо переделать _container.InstantiatePrefab чтобы 
        // основным был тот который принимает gameobject.
    }
}