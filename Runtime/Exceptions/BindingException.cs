using System;

namespace Uniject.Exceptions
{
    public class BindingException : ApplicationException
    {
        public BindingException(string message) : base(message)
        {
        }
    }
}