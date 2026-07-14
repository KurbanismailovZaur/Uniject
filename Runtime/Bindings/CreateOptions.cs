using UnityEngine;

namespace Uniject.Bindings
{
    public readonly struct CreateOptions
    {
        public readonly string gameObjectName;
        public readonly Transform underTransform;
        public readonly Transform parentTransformForGameObjects;
        public readonly Transform contextTransform;

        public CreateOptions(string gameObjectName, Transform underTransform, 
            Transform parentTransformForGameObjects, Transform contextTransform)
        {
            this.gameObjectName = gameObjectName;
            this.underTransform = underTransform;
            this.parentTransformForGameObjects = parentTransformForGameObjects;
            this.contextTransform = contextTransform;
        }
    }
}