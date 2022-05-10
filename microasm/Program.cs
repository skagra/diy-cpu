namespace microasm
{
   public class MicroAsmRunner
   {
      private const string OP_CODE_MAP_DEFINITION = "OpCodeMap.txt";
      private const string MODE_MAP_DEFINITION = "ModeMap.txt";
      private const string MICROCODE_DEFINITION = "uROM.txt";
      private const string OP_CODE_MAP_ROM = "OpCodeMap.bin";
      private const string MODE_MAP_ROM = "ModeMap.bin";
      private const string MICROCODE_ROM = "uROM";

      public static void Main(string[] args)
      {
         Console.WriteLine("MicroAsm");
         Console.WriteLine("--------");
         Console.WriteLine();

         if (args.Length != 2)
         {
            Console.WriteLine("Usage: microasm source-directory output-directory");
            return;
         }

         var inputDir = args[0];
         var outputDir = args[1];

         Console.WriteLine($"Input dir: '{Path.GetFullPath(inputDir)}'");
         Console.WriteLine($"Output dir: '{Path.GetFullPath(outputDir)}'");
         Console.WriteLine();

         try
         {
            if (!Directory.Exists(outputDir))
            {
               Directory.CreateDirectory(outputDir);
            }

            var opCodeMap = new MappingRom(Path.Join(inputDir, OP_CODE_MAP_DEFINITION));
            var modeMap = new MappingRom(Path.Join(inputDir, MODE_MAP_DEFINITION));

            var ma = new MicroAsm(opCodeMap, modeMap, Path.Join(inputDir, MICROCODE_DEFINITION));

            Console.WriteLine("mCode Map");
            Console.WriteLine("---------");
            Console.WriteLine();
            Console.WriteLine(opCodeMap);

            Console.WriteLine("mMode Map");
            Console.WriteLine("--------");
            Console.WriteLine();
            Console.WriteLine(modeMap);

            Console.WriteLine("Flag Symbol Table");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine(ma.DumpFlagSymbols());
            Console.WriteLine();

            Console.WriteLine("uOps Symbol Table");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine(ma.DumpUCopsSymbols());
            Console.WriteLine();

            Console.WriteLine("Label Symbol Table");
            Console.WriteLine("------------------");
            Console.WriteLine();
            Console.WriteLine(ma.DumpLabelSymbols());
            Console.WriteLine();

            Console.WriteLine("Generated mMode Map");
            Console.WriteLine("-------------------");
            Console.WriteLine();
            Console.WriteLine(ma.DumpModeMap());
            Console.WriteLine();

            Console.WriteLine("Generated mCode Map");
            Console.WriteLine("-------------------");
            Console.WriteLine();
            Console.WriteLine(ma.DumpOpCodeMap());
            Console.WriteLine();

            Console.WriteLine("Generated uCode");
            Console.WriteLine("---------------");
            Console.WriteLine();
            Console.WriteLine($"uROM size: {ma.GetRomSizeWords()} words");
            Console.WriteLine();
            Console.WriteLine(ma.DumpOutputLog());
            Console.WriteLine();

            ma.WriteOpCodeMappingFile(Path.Join(outputDir, OP_CODE_MAP_ROM));
            ma.WriteModeMappingFile(Path.Join(outputDir, MODE_MAP_ROM));
            ma.WriteROMFile(Path.Join(outputDir, MICROCODE_ROM));
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
      }
   }
}