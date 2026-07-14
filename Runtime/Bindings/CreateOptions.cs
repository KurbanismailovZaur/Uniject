using UnityEngine;

namespace Uniject.Bindings
{
    public readonly struct CreateOptions
    {
        public readonly string gameObjectName;
        public readonly Transform underTransform;
        public readonly Transform parentTransformForGameObjects;

        public static CreateOptions Default => new (null, null, null);

        public CreateOptions(string gameObjectName, Transform underTransform, 
            Transform parentTransformForGameObjects)
        {
            this.gameObjectName = gameObjectName;
            this.underTransform = underTransform;
            this.parentTransformForGameObjects = parentTransformForGameObjects;
        }
    }
}