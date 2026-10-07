using UnityEngine;

namespace Uniject
{
    internal static class StaticCollections
    {
        internal static CollectionPool collectionPool = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            collectionPool.Dispose();
            collectionPool = new CollectionPool();
        }
    }
}