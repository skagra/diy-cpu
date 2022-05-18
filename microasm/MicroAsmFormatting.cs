using System.Text;

namespace MicroAsm
{
   using static MicroAsmConstants;

   public static class MicroAsmFormatting
   {
      public static string ByteToBitString(byte value)
      {
         return Convert.ToString(value, 2).PadLeft(8, '0');
      }

      public static string CreateByteArrayString(byte[] value)
      {
         var result = new StringBuilder();

         for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
         {
            result.Append(ByteToBitString(value[byteIndex]).Replace("0", ".") + " ");
         }

         result.Append("\t");

         for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
         {
            result.Append($"{value[byteIndex]:X2}");
            if (byteIndex != 0)
            {
               result.Append(" ");
            }
         }

         return result.ToString();
      }

      public static string DumpSymbols(Dictionary<string, byte[]> symbols)
      {
         var result = new StringBuilder();
         symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
         {
            var value = symbols[k];
            result.AppendLine($"{k,-40} {CreateByteArrayString(value)}");
         });
         return result.ToString();
      }

      public static string DumpMap(Dictionary<string, int> symbols)
      {
         var result = new StringBuilder();
         symbols.Keys.OrderBy(k => k).ToList().ForEach(
             k =>
             {
                var value = symbols[k];
                result.AppendLine($"{k,-20} {value:X4}");
             }
         );

         return result.ToString();
      }

      public static string TrimAndStripComments(string line)
      {
         var commentIndex = line.IndexOf(COMMENT_CHARACTERS);
         if (commentIndex != -1)
         {
            line = line.Substring(0, commentIndex);

         }
         return line.Trim();
      }
   }
}