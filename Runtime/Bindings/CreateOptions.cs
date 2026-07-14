using Uniject.Contexts;
using UnityEngine;

namespace Uniject.Bindings
{
    public readonly struct CreateOptions
    {
        public readonly string gameObjectName;
        public readonly Transform underTransform;
        public readonly Transform parentTransformForGameObjects;
        public readonly Context context;

        public static CreateOptions Default => new (null, null, null, null);

        public CreateOptions(string gameObjectName, Transform underTransform, 
            Transform parentTransformForGameObjects, Context context)
        {
            this.gameObjectName = gameObjectName;
            this.underTransform = underTransform;
            this.parentTransformForGameObjects = parentTransformForGameObjects;
            this.context = context;
        }
    }
}