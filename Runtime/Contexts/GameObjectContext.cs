using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public class GameObjectContext : Context
    {
        public override void Build()
        {
            Container = new Container();
            base.Build();
        }
    }
}
