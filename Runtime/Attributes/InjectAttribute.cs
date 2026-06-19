using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Uniject.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    public class InjectAttribute : PreserveAttribute
    {
    }
}