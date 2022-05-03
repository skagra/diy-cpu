using System;
using System.Text;

namespace microasm
{
   public class VertPrinter
   {
      private readonly int _rows;
      private readonly int _columns;

      private readonly char[][] _buffer;

      public VertPrinter(int rows, int columns)
      {
         _rows = rows;
         _columns = columns*2;

         _buffer = new char[_rows][];
         for (int i=0; i<_buffer.Length; i++)
         {
            _buffer[i] = new char[_columns];
            for (int col=0; col<_columns; col++)
            {
               _buffer[i][col] = ' ';
            }
         }
      }

      public void Print(string toPrint, int column)
      {
         if (toPrint.Length > _rows)
         {
            toPrint = toPrint.Trim().Substring(0, _rows);
         }
         if (toPrint.Length < _rows)
         {
            toPrint = toPrint.PadLeft(_rows);
         }

         for (int row=0; row< toPrint.Length; row++)
         {
            _buffer[row][column*2] = toPrint[row];
         }

      }

      public override string ToString()
      {
         var result = new StringBuilder(); 
         for (int row = 0; row < _rows; row++)
         {
            for (int col = 0; col < _columns; col++)
         {
            
               result.Append(_buffer[row][col]);
            }
            result.AppendLine();
         }
         return result.ToString();
      }
   }
}
