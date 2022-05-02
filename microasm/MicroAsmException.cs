using System;

namespace microasm
{
    public class MicroAsmException : Exception
    {
        public MicroAsmException(string message, string line, int lineNumber) : base($"Error: '{message}' at line '{lineNumber}', '{line}' ")
        {
        }

        public MicroAsmException(string message) : base(message)
        {
        }
    }
}
