using Uniject.Contexts;
using UnityEngine;

namespace Uniject.Bindings
{
    public readonly struct CreateOptions
    {
        public readonly string gameObjectName;
        public readonly Transform underTransform;
        public readonly Transform parentForGameObjects;
        public readonly Context context;

        public static CreateOptions Default => new (null, null, null, null);

        public CreateOptions(string gameObjectName, Transform underTransform, 
            Transform parentForGameObjects, Context context)
        {
            this.gameObjectName = gameObjectName;
            this.underTransform = underTransform;
            this.parentForGameObjects = parentForGameObjects;
            this.context = context;
        }
    }
}