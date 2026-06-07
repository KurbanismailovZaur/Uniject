using System;

namespace Uniject.Exceptions
{
    public class InjectException : ApplicationException
    {
        public InjectException(string message) : base(message)
        {
        }
    }
}