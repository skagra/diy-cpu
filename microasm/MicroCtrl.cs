
namespace MicroAsm
{
   using static MicroAsmConstants;
   using static MicroAsmFormatting;

   public class MicroCtrl
   {
      // For efficient conversion of bit strings to byte values
      private static readonly byte[] BytePowers = { 128, 64, 32, 16, 8, 4, 2, 1 };

      private static readonly char[] SPLIT_CHARS = new char[] { ' ', '\t' };

      private readonly Dictionary<string, byte[]> _flagSymbols = new();

      private readonly string _sourceFile;

      public MicroCtrl(string sourceFile)
      {
         _sourceFile = sourceFile;
         Parse();
      }

      private byte ParseBinaryByteString(string valueString)
      {
         if (valueString.Length != 8)
         {
            throw new MicroAsmException($"Invalid binary byte string, the string is not 8 characters long '{valueString}'");
         }

         byte result = 0;
         for (int valueStringIndex = 0; valueStringIndex < 8; valueStringIndex++)
         {
            var valueChar = valueString[valueStringIndex];
            switch (valueChar)
            {
               case '1':
               case 'x':
               case 'X':
                  result |= BytePowers[valueStringIndex];
                  break;
               case '.':
               case '-':
               case '_':
               case '0':
                  break;
               default:
                  throw new MicroAsmException($"Invalid character in binary string '{valueChar}'");
            }
         }

         return result;
      }

      private void ParseFlagsLine(string line, int lineNumber)
      {
         var flagsParts = line.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

         if (flagsParts.Length < 2)
         {
            throw new MicroAsmException(
                $"Flag lines must consist of two white-space separated values",
                line, lineNumber, _sourceFile);
         }

         var symbolString = flagsParts[0];
         var valueString = string.Join(' ', flagsParts[1..]).Replace(" ", "");

         if (valueString.Length != FLAGS_SIZE_IN_BYTES * 8)
         {
            throw new MicroAsmException(
                $"Flag value must be '{FLAGS_SIZE_IN_BYTES * 8}' characters long",
                line, lineNumber, _sourceFile);
         }

         var value = new byte[WORD_SIZE_IN_BYTES];
         for (var valByteIndex = 0; valByteIndex < FLAGS_SIZE_IN_BYTES; valByteIndex++)
         {
            value[WORD_SIZE_IN_BYTES - valByteIndex - 1] = ParseBinaryByteString(
                valueString.Substring(valByteIndex * 8, 8)
            );
         }

         try
         {
            _flagSymbols.Add(symbolString, value);
         }
         catch (ArgumentException)
         {
            throw new MicroAsmException($"Duplicate symbol '{symbolString}'",
                line, lineNumber, _sourceFile);
         }
      }

      public bool TryGetValue(string symbol, out byte[] result)
      {
         return _flagSymbols.TryGetValue(symbol, out result);
      }

      private void Parse()
      {
         int lineNumber = 1;

         foreach (var line in File.ReadLines(_sourceFile))
         {
            var trimmedLine = TrimAndStripComments(line);

            // Skip empty lines and comment lies
            if (trimmedLine.Length > 0)
            {
               ParseFlagsLine(trimmedLine, lineNumber);
            }
            lineNumber++;
         }
      }

      public override string ToString()
      {
         return DumpSymbols(_flagSymbols);
      }
   }
}
