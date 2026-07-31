using System;
using System.IO;
using Uniject.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject
{
    internal static class StaticCollections
    {
        internal static CollectionPool collectionPool = new();
    }
}