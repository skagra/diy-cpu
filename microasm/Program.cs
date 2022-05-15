namespace microasm
{
    public class MicroAsmRunner
    {
        private const string MCODE_OP_DECODER_DEFINITION = "mOpDecoder.txt";
        private const string MCODE_MODE_DECODER_DEFINITION = "mModeDecoder.txt";
        private const string UCODE_DEFINITION = "uCode.txt";
        private const string UCTRL_DEFINITION = "uCtrl.txt";
        private const string UOPS_DEFINITION = "uOps.txt";
        private const string MCODE_OP_DECODER_ROM = "mOpDecoder.bin";
        private const string MCODE_MODE_DECODER_ROM = "mModeDecoder.bin";
        private const string MICROCODE_ROM_PREFIX = "uROM";

        public static void Main(string[] args)
        {
            Console.WriteLine("MicroAsm");
            Console.WriteLine("--------");
            Console.WriteLine();

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: microasm <source-directory> <output-directory>");
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

                var mCodeOpDecoderDefinition = new DecoderRom(Path.Join(inputDir, MCODE_OP_DECODER_DEFINITION));
                var mCodeModeDecoderDefinition = new DecoderRom(Path.Join(inputDir, MCODE_MODE_DECODER_DEFINITION));

                var uCtrlDefinition = new MicroCtrl(Path.Join(inputDir, UCTRL_DEFINITION));
                var uOpsDefinition = new MicroOps(uCtrlDefinition, Path.Join(inputDir, UOPS_DEFINITION));

                var microAsm = new MicroAsm(mCodeOpDecoderDefinition, mCodeModeDecoderDefinition, uCtrlDefinition, uOpsDefinition, Path.Join(inputDir, UCODE_DEFINITION));

                Console.WriteLine("mCode Decoder");
                Console.WriteLine("-------------");
                Console.WriteLine();
                Console.WriteLine(mCodeOpDecoderDefinition);

                Console.WriteLine("mMode Decoder");
                Console.WriteLine("-------------");
                Console.WriteLine();
                Console.WriteLine(mCodeModeDecoderDefinition);

                Console.WriteLine("uControl Symbol Table");
                Console.WriteLine("---------------------");
                Console.WriteLine();
                Console.WriteLine(uCtrlDefinition.ToString());
                Console.WriteLine();

                Console.WriteLine("uOps Symbol Table");
                Console.WriteLine("-----------------");
                Console.WriteLine();
                Console.WriteLine(uOpsDefinition.ToString());
                Console.WriteLine();

                Console.WriteLine("uLabel Symbol Table");
                Console.WriteLine("-------------------");
                Console.WriteLine();
                Console.WriteLine(microAsm.DumpLabelSymbols());
                Console.WriteLine();

                Console.WriteLine("uRom");
                Console.WriteLine("----");
                Console.WriteLine();
                Console.WriteLine($"Size: {microAsm.GetURomSizeWords()} words");
                Console.WriteLine();
                Console.WriteLine(microAsm.DumpURom());
                Console.WriteLine();

                microAsm.WriteMOpDecoderRom(Path.Join(outputDir, MCODE_OP_DECODER_ROM));
                microAsm.WriteMModeDecoderRom(Path.Join(outputDir, MCODE_MODE_DECODER_ROM));
                microAsm.WriteUCodeRom(Path.Join(outputDir, MICROCODE_ROM_PREFIX));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}