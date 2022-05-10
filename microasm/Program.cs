namespace microasm
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

         var opCodeMap = new MappingRom("../uCode/opCodeMap.txt");
         var addrModeMap = new MappingRom("../uCode/AddrModeMap.txt");

         Console.WriteLine(opCodeMap);
         Console.WriteLine(addrModeMap);

         // try
         // {
         var outputDir = Path.GetDirectoryName(outputFile);
         if (!Directory.Exists(outputDir))
         {
            Directory.CreateDirectory(outputDir);
         }

         var ma = new MicroAsm(opCodeMap, addrModeMap, inputFile);

         ma.DumpFlagSymbols();
         Console.WriteLine();

         ma.DumpUCopsSymbols();
         Console.WriteLine();

         ma.DumpLabelSymbols();
         Console.WriteLine();

         // ma.DumpOpsAddrs();
         // Console.WriteLine();

         ma.DumpOutputLog();

         ma.WriteOpCodeMappingFile(Path.Join(outputDir, "opcodemap.bin"));
         ma.WriteModeMappingFile(Path.Join(outputDir, "modemap.bin"));

         ma.WriteROMFile(outputFile);
         // }
         // catch (Exception e)
         // {
         //    Console.WriteLine(e.Message);
         // }
      }
   }
}