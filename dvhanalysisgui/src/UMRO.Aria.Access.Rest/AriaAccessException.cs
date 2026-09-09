using System;

namespace UMRO.Aria.Access.Rest
{
    public class AriaAccessException : Exception
    {
        public AriaAccessException()
        {
        }

        public AriaAccessException(string message)
            : base(message)
        {
        }

        public AriaAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
