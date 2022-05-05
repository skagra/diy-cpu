﻿namespace microasm
{
   public class MicroAsmRunner
   {
      public static void Main(string[] args)
      {
         Console.WriteLine("MicroAsm");
         Console.WriteLine("--------");
         Console.WriteLine();

         if (args.Length != 2)
         {
            Console.WriteLine("Usage: microasm source-file rom-file");
            return;
         }

         var inputFile = args[0];
         var outputFile = args[1];

         Console.WriteLine($"Input file: '{Path.GetFullPath(inputFile)}'");
         Console.WriteLine($"Output file: '{Path.GetFullPath(outputFile)}'");
         Console.WriteLine();

         try
         {
            var outputDir=Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(outputDir)) {
               Directory.CreateDirectory(outputDir);
            }

            var ma = new MicroAsm(inputFile);

            ma.DumpFlagSymbols();
            Console.WriteLine();

            ma.DumpUCopsSymbols();
            Console.WriteLine();

            ma.DumpLabelSymbols();
            Console.WriteLine();

            ma.DumpOpsAddrs();
            Console.WriteLine();

            ma.DumpOutputLog();

            ma.WriteROMFile(outputFile);
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
      }
   }
}