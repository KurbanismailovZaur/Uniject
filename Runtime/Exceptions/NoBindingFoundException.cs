using System;

namespace Uniject.Exceptions
{
    public class NoBindingFoundException : Exception
    {
        public NoBindingFoundException(string message) : base(message) { }
    }
}
