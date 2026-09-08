// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Aria.Access.Rest.dll (version 1.0.0.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

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
