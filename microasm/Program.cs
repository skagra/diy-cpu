namespace MicroAsm
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

            if (args.Length != 6)
            {
                Console.WriteLine("Usage: microasm <flags-bytes> <ucode-addr-bytes> <ucode-rom-word-size-bytes> <ucode-rom-addr-width-bytes> <source-directory> <output-directory>");
                return;
            }

            var flagsBytesString = args[0];
            var uCodeAddrBytesString = args[1];
            var uCodeROMWordSizeBytesString = args[2];
            var uCodeROMAddrWidthBytesString = args[3];
            var inputDir = args[4];
            var outputDir = args[5];

            Console.WriteLine($"uCode control lines: '{flagsBytesString}' bytes");
            Console.WriteLine($"uCode address width: '{uCodeAddrBytesString}' bytes");
            Console.WriteLine($"uCode ROMs data width: '{uCodeROMWordSizeBytesString}' bytes");
            Console.WriteLine($"uCode ROMs address width: '{uCodeROMAddrWidthBytesString}' bytes");
            Console.WriteLine($"Input dir: '{Path.GetFullPath(inputDir)}'");
            Console.WriteLine($"Output dir: '{Path.GetFullPath(outputDir)}'");
            Console.WriteLine();

            try
            {

                var flagsBytes = int.Parse(flagsBytesString);
                var uCodeAddrBytes = int.Parse(uCodeAddrBytesString);
                var uCodeROMWordSizeBytes = int.Parse(uCodeROMWordSizeBytesString);
                var uCodeROMAddrWidthBytes = int.Parse(uCodeROMWordSizeBytesString);

                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var mCodeOpDecoderDefinition = new DecoderRom(Path.Join(inputDir, MCODE_OP_DECODER_DEFINITION));
                var mCodeModeDecoderDefinition = new DecoderRom(Path.Join(inputDir, MCODE_MODE_DECODER_DEFINITION));

                var uCtrlDefinition = new MicroCtrl(Path.Join(inputDir, UCTRL_DEFINITION), flagsBytes, uCodeAddrBytes);
                var uOpsDefinition = new MicroOps(uCtrlDefinition, Path.Join(inputDir, UOPS_DEFINITION), flagsBytes + uCodeAddrBytes);

                var microAsm = new MicroCode(mCodeOpDecoderDefinition, mCodeModeDecoderDefinition,
                    uCtrlDefinition, uOpsDefinition,
                    Path.Join(inputDir, UCODE_DEFINITION),
                    flagsBytes, uCodeAddrBytes, uCodeROMWordSizeBytes, uCodeROMAddrWidthBytes);

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
                Console.WriteLine($"uOps used: {microAsm.GetCountOfMicroOpsUsed()}");
                Console.WriteLine();
                Console.WriteLine(microAsm.DumpURom());
                Console.WriteLine();

                // TODO: Validation
                Console.WriteLine();
                Console.WriteLine("Validating all mCodes implemented have at least one mMode decoding defined");
                Console.WriteLine("Validating all mCodes implemented have at least one mCode decoding defined");
                Console.WriteLine();

                Console.Write($"Writing '{MCODE_OP_DECODER_ROM}'...");
                microAsm.WriteMOpDecoderRom(Path.Join(outputDir, MCODE_OP_DECODER_ROM));
                Console.WriteLine("done");

                Console.Write($"Writing '{MCODE_MODE_DECODER_ROM}'...");
                microAsm.WriteMModeDecoderRom(Path.Join(outputDir, MCODE_MODE_DECODER_ROM));
                Console.WriteLine("done");

                Console.Write($"Writing '{MICROCODE_ROM_PREFIX}' files...");
                microAsm.WriteUCodeRom(Path.Join(outputDir, MICROCODE_ROM_PREFIX));
                Console.WriteLine("done");

                Console.WriteLine();
                Console.WriteLine("All done.");
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}