using System.Text;

namespace MicroAsm
{
   using static MicroAsmConstants;

   public class DecoderRom
   {
      private const int MAPPING_SIZE = 256;

      private static readonly char[] SPLIT_CHARS = new char[] { ' ', '\t' };

      private readonly string[] _mapping = new string[MAPPING_SIZE];

      private readonly string _fileName;

      public DecoderRom(string fileName)
      {
         _fileName = fileName;
         ParseDecoderFile();
      }

      public bool ContainsDecodingFor(string symbol)
      {
         return _mapping.Contains(symbol);
      }

      private void ParseDecoderFile()
      {
         int lineNumber = 1;
         foreach (var line in File.ReadLines(_fileName))
         {
            var lineParts = line.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lineParts.Length > 0 && !line.StartsWith(COMMENT_CHARACTERS))
            {
               if (lineParts.Length > 1)
               {
                  try
                  {
                     var symbol = lineParts[0];
                     lineParts.Skip(1).ToList().ForEach(value =>
                     {
                        var index = byte.Parse(value, System.Globalization.NumberStyles.HexNumber);
                        if (_mapping[index] != null)
                        {
                           throw new MicroAsmException($"A mapping to this value has already been seen '{index:X2}'.", line, lineNumber, _fileName);
                        }
                        _mapping[index] = symbol;
                     });
                  }
                  catch (IndexOutOfRangeException)
                  {
                     throw new MicroAsmException($"Mapping value must be less than {_mapping.Length}.", line, lineNumber, _fileName);
                  }
                  catch (FormatException)
                  {
                     throw new MicroAsmException($"Values must be given as hex bytes.", line, lineNumber, _fileName);
                  }
               }
               else
               {
                  throw new MicroAsmException("Rom mapping lines must have a least one value.", line, lineNumber, _fileName);
               }
            }
            lineNumber++;
         }
      }

      public string ResolveIndex(int index)
      {
         return _mapping[index];
      }

      public override string ToString()
      {
         var result = new StringBuilder();

         for (var index = 0; index < _mapping.Length; index++)
         {
            if (_mapping[index] != null)
            {
               result.AppendLine($"{index:X2}\t{_mapping[index]}");
            }
         }

         return result.ToString();
      }
   }
}