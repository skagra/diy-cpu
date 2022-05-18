namespace MicroAsm
{
   public class MicroAsmException : Exception
   {
      public MicroAsmException(string message, string line, int lineNumber, string fileName) : this($"Error: {message} at line {lineNumber}, '{line}' [{Path.GetFullPath(fileName)}:{lineNumber}]")
      {
      }

      public MicroAsmException(string message, string line, int lineNumber) : this($"Error: {message} at line {lineNumber}, '{line}'")
      {
      }

      public MicroAsmException(string message) : base(message)
      {
      }


   }
}
