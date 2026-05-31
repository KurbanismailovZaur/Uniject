using System;
using UnityEngine;

namespace Uniject.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}