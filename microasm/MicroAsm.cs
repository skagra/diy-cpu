namespace microasm
{
    using static MicroAsmConstants;
    using static MicroAsmFormatting;

    public class MicroAsm
    {
        // Size of the output ROM in words
        private const int WORD_COUNT = 0x10000;

        // Size of the output ROM in bytes
        private const int TOTAL_ROM_SIZE_BYTES = WORD_SIZE_IN_BYTES * WORD_COUNT;

        // Width in bytes of data in each output ROM file
        private const int ROM_DATA_WIDTH_BYTES = 2;

        // Size of each ROM
        private const int ROM_SIZE_BYTES = ROM_DATA_WIDTH_BYTES * 65536;

        // Identifies uCode implementing a machine code op
        private const string OPCODE = ".opcode";

        // Identifies uCode implementation a machine code addressing mode
        private const string MODE = ".mode";

        // Label a location so it may be referred to later
        private const string LABEL = ".label";

        // Symbols tables filled in as the uCode file is parsed then used to generate ROM output
        // Each symbol value is in output (little endian) order and is the full size of uCode word

        private readonly Dictionary<string, byte[]> _labelSymbols = new();
        private readonly Dictionary<string, byte[]> _codeSymbols = new();
        private readonly Dictionary<string, int> _opCodeRoutineAddresses = new();
        private readonly Dictionary<string, int> _modeRoutineAddresses = new();

        private readonly DecoderRom _opCodeMappingROM;
        private readonly DecoderRom _modeMappingROM;

        private readonly MicroCtrl _microCtrl;
        private readonly MicroOps _microOps;

        private readonly List<string> _outputLog = new();

        private readonly byte[] _ROM = new byte[TOTAL_ROM_SIZE_BYTES];

        private int _romAddress = 0;

        private readonly string _sourceFile;

        public MicroAsm(DecoderRom opCodeMappingROM,
            DecoderRom modeMappingROM,
            MicroCtrl microCtrl,
            MicroOps microOps,
            string sourceFile)
        {

            _opCodeMappingROM = opCodeMappingROM;
            _modeMappingROM = modeMappingROM;
            _sourceFile = sourceFile;
            _microCtrl = microCtrl;
            _microOps = microOps;

            Parse();
        }

        public string DumpLabelSymbols()
        {
            return DumpSymbols(_labelSymbols);
        }

        public string DumpURom()
        {
            return (string.Join('\n', _outputLog));
        }

        public int GetURomSizeWords()
        {
            return _romAddress;
        }

        private byte[] ResolveSymbol(string symbol, string line, int lineNumber)
        {
            byte[] result;

            if (!(_microCtrl.TryGetValue(symbol, out result) ||
                  _microOps.TryGetValue(symbol, out result) ||
                  _labelSymbols.TryGetValue(symbol, out result) ||
                  _codeSymbols.TryGetValue(symbol, out result)))
            {
                throw new MicroAsmException($"Symbol not found '{symbol}'", line, lineNumber, _sourceFile);
            }

            return result;
        }

        private static void Or(byte[] source, byte[] dest)
        {
            for (var i = 0; i < source.Length; i++)
            {
                dest[i] |= source[i];
            }
        }

        private void ProcessModeOrOpCodeLine(string line, int lineNumber, Dictionary<string, int> addresses, int pass)
        {
            var parts = line.Split(new char[] { ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length != 2)
            {
                throw new MicroAsmException($"Line must consist of two whitespace separated values", line, lineNumber, _sourceFile);
            }

            var lineType = parts[0];
            if (pass == 0)
            {
                try
                {
                    addresses[parts[1]] = _romAddress;
                }
                catch (KeyNotFoundException)
                {
                    throw new MicroAsmException($"'{lineType}' value not found",
                        line, lineNumber, _sourceFile
                    );
                }
            }
            else
            {
                _outputLog.Add($"\n{lineType} {parts[1]}");
            }
        }

        // TODO: Assumes a 16 bit address space
        private void ParseCodeLinePass0(string line, int lineNumber)
        {
            if (line.StartsWith(LABEL))
            {
                var labelParts = line.Split(new char[] { ' ', '\t' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var symbol = labelParts[1];
                try
                {
                    var addressWord = new byte[WORD_SIZE_IN_BYTES];
                    addressWord[0] = (byte)(_romAddress & 0xFF); // Low byte
                    addressWord[1] = (byte)((_romAddress >> 8) & 0xFF); // High
                    for (var offset = 2; offset < WORD_SIZE_IN_BYTES; offset++)
                    {
                        addressWord[offset] = 0;
                    }
                    _labelSymbols.Add(symbol, addressWord);
                }
                catch
                {
                    throw new MicroAsmException($"Duplicate symbol '{symbol}'",
                        line, lineNumber, _sourceFile
                    );
                }
            }
            else
            {
                if (line.StartsWith(OPCODE))
                {
                    ProcessModeOrOpCodeLine(line, lineNumber, _opCodeRoutineAddresses, 0);
                }
                else if (line.StartsWith(MODE))
                {
                    ProcessModeOrOpCodeLine(line, lineNumber, _modeRoutineAddresses, 0);
                }
                else
                {
                    _romAddress += 1;
                }
            }
        }

        private string FormatCodeLogLine(int byteRomAddress, string line)
        {
            var logLine = $"{_romAddress:X4}    {line,-30}";
            if (line.Length > 28)
            {
                logLine += $"\n{' ',38}";
            }
            logLine += $"{CreateByteArrayString(_ROM[byteRomAddress..(byteRomAddress + WORD_SIZE_IN_BYTES)])}";
            return logLine;
        }

        private void ParseCodeLinePass1(string line, int lineNumber)
        {
            if (!line.StartsWith(LABEL))
            {
                if (line.StartsWith(OPCODE))
                {
                    ProcessModeOrOpCodeLine(line, lineNumber, _opCodeRoutineAddresses, 1);
                }
                else if (line.StartsWith(MODE))
                {
                    ProcessModeOrOpCodeLine(line, lineNumber, _modeRoutineAddresses, 1);
                }
                else
                {
                    var codeParts = line.Split(new char[] { ' ', '\t', '|' },
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var value = new byte[WORD_SIZE_IN_BYTES];
                    foreach (var codePart in codeParts)
                    {
                        Or(ResolveSymbol(codePart, line, lineNumber), value);
                    }

                    int byteRomAddress = _romAddress * WORD_SIZE_IN_BYTES;
                    var outputLogByteArray = new byte[WORD_SIZE_IN_BYTES];
                    for (var offset = 0; offset < WORD_SIZE_IN_BYTES; offset++)
                    {
                        _ROM[byteRomAddress + offset] = value[offset];
                        outputLogByteArray[offset] = value[offset];
                    }

                    _outputLog.Add(FormatCodeLogLine(byteRomAddress, line));
                    _romAddress++;
                }
            }
            else
            {
                _outputLog.Add($"\n{line,-40}");
            }
        }



        private void Parse()
        {
            for (int pass = 0; pass < 2; pass++)
            {
                int lineNumber = 1;
                _romAddress = 0;
                foreach (var line in File.ReadLines(_sourceFile))
                {
                    var trimmedLine = TrimAndStripComments(line);

                    if (trimmedLine.Length > 0)
                    {
                        if (pass == 0)
                        {
                            ParseCodeLinePass0(trimmedLine, lineNumber);
                        }
                        else
                        {
                            ParseCodeLinePass1(trimmedLine, lineNumber);
                        }
                    }
                    lineNumber++;
                }
            }
        }

        // TODO: Does not allow for number of bytes per word being smaller than width of ROM
        public void WriteUCodeRom(string romFile)
        {
            int numROMFiles = (TOTAL_ROM_SIZE_BYTES + ROM_SIZE_BYTES - 1) / ROM_SIZE_BYTES;
            var writers = new BinaryWriter[numROMFiles];
            for (var romIndex = 0; romIndex < numROMFiles; romIndex++)
            {
                writers[romIndex] = new BinaryWriter(File.Open($"{romFile}-{romIndex}.bin", FileMode.Create));
            }

            int byteIndex = 0;
            while (byteIndex < TOTAL_ROM_SIZE_BYTES)
            {
                for (var romIndex = 0; romIndex < numROMFiles && byteIndex < TOTAL_ROM_SIZE_BYTES; romIndex++)
                {
                    for (int byteInRom = 0; byteInRom < ROM_DATA_WIDTH_BYTES; byteInRom++)
                    {
                        writers[romIndex].Write(_ROM[byteIndex]);
                        byteIndex++;
                    }
                }
            }

            for (var romIndex = 0; romIndex < numROMFiles; romIndex++)
            {
                writers[romIndex].Close();
            }
        }

        public void WriteMOpDecoderRom(string fileName)
        {
            WriteDecoderRom(_opCodeMappingROM, _opCodeRoutineAddresses, fileName);
        }

        public void WriteMModeDecoderRom(string fileName)
        {
            WriteDecoderRom(_modeMappingROM, _modeRoutineAddresses, fileName);
        }

        private void WriteDecoderRom(DecoderRom decoderRom, Dictionary<string, int> symbolAddresses, string romFile)
        {
            var writer = new BinaryWriter(File.Open(romFile, FileMode.Create));

            // TODO: HACKY
            for (var index = 0; index < 256; index++)
            {
                var symbol = decoderRom.ResolveIndex(index);
                try
                {
                    if (symbol != null)
                    {
                        int addr = symbolAddresses[symbol];
                        writer.Write((byte)(addr & 0xFF));
                        writer.Write((byte)((addr >> 8) & 0xFF));
                    }
                    else
                    {
                        writer.Write((byte)0xFF); // Flags an error condition
                        writer.Write((byte)0xFF);
                    }
                }
                catch (KeyNotFoundException)
                {
                    throw new MicroAsmException($"The symbol '{symbol}' was not found in the decoder");
                }
            }
            writer.Close();
        }
    }
}
