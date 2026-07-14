using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public class SceneContext : Context
    {
        protected void Start() => Run();
    }
}
