using System;
using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Exceptions
{
    public class NoBindingFoundException : Exception
    {
        public NoBindingFoundException(string message) : base(message) { }
    }
}
