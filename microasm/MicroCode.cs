namespace MicroAsm
{
    using static MicroAsmFormatting;

    public class MicroCode
    {
        // Size of the output ROM in words
        private const int WORD_COUNT = 0x10000; // 64K

        // Identifies uCode implementing a machine code op
        private const string OPCODE = ".opcode";

        // Identifies uCode implementation a machine code addressing mode
        private const string MODE = ".mode";

        // Label a location so it may be referred to later
        private const string LABEL = ".label";

        private static readonly char[] SPLIT_CHARS = new char[] { ' ', '\t', '|' };

        private static readonly char[] CODE_SPLIT_CHARS = new char[] { ' ', '\t', '|' };

        // Symbols tables filled in as the uCode file is parsed then used to generate ROM output
        // Each symbol value is in output (little endian) order and is the full size of uCode word
        private readonly Dictionary<string, byte[]> _labelSymbols = new();
        //   private readonly Dictionary<string, byte[]> _codeSymbols = new();
        private readonly Dictionary<string, int> _opCodeRoutineAddresses = new();
        private readonly Dictionary<string, int> _modeRoutineAddresses = new();

        private readonly DecoderRom _opCodeMappingROM;
        private readonly DecoderRom _modeMappingROM;

        private readonly MicroCtrl _microCtrl;
        private readonly MicroOps _microOps;

        private HashSet<string> _uOpsUsed = new();

        private readonly List<string> _outputLog = new();

        private readonly byte[] _ROM;

        private int _romAddress = 0;

        private readonly string _sourceFile;

        // Bytes in each uCode instruction
        private readonly int _uCodeFlagsBytes;

        // Bytes in address in uCode ROM
        private readonly int _uCodeAddrBytes;

        // Total width of a word across the uCode ROMs (_flagsBytes+_uCodeAddrBytes)
        private readonly int _uCodeWordSizeBytes;

        // Size of the output ROM in bytes (this will be split across multiple ROM files)
        // (_wordSizeBytes * WORD_COUNT)
        private readonly int _compositeROMSizeBytes;

        // Width in address in each output ROM file
        private readonly int _uCodeROMWordSizeBytes;

        // Size of each ROM
        private readonly int _individualROMSizeBytes;

        private readonly int _uCodeROMAddrWidthBytes;

        public MicroCode(DecoderRom opCodeMappingROM,
            DecoderRom modeMappingROM,
            MicroCtrl microCtrl,
            MicroOps microOps,
            string sourceFile,
            int uCodeFlagsBytes, // bytes in flags (control lines)
            int uCodeAddrBytes, // bytes in uCodeAddresses
            int uCodeROMWordSizeBytes,
            int uCodeROMAddrWidthBytes) // Width of word in each ROM - slicing up the output
        {

            _opCodeMappingROM = opCodeMappingROM;
            _modeMappingROM = modeMappingROM;
            _sourceFile = sourceFile;
            _microCtrl = microCtrl;
            _microOps = microOps;
            _uCodeROMWordSizeBytes = uCodeROMWordSizeBytes;
            _uCodeROMAddrWidthBytes = uCodeROMAddrWidthBytes;

            _uCodeFlagsBytes = uCodeFlagsBytes;
            _uCodeAddrBytes = uCodeAddrBytes;
            _uCodeWordSizeBytes = uCodeFlagsBytes + uCodeAddrBytes;

            _compositeROMSizeBytes = 1280; // _uCodeWordSizeBytes * WORD_COUNT;  // TODO
            _ROM = new byte[_compositeROMSizeBytes];

            _individualROMSizeBytes = (int)(Math.Pow(2, uCodeROMAddrWidthBytes * 8)) * uCodeROMWordSizeBytes; // TODO: Double conversion is this safe!

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

        public int GetCountOfMicroOpsUsed()
        {
            return _uOpsUsed.Count();
        }

        private byte[] ResolveSymbol(string symbol, string line, int lineNumber)
        {
            byte[] result;

            if (!(_microCtrl.TryGetValue(symbol, out result) ||
                  _microOps.TryGetValue(symbol, out result) ||
                  _labelSymbols.TryGetValue(symbol, out result)))
            {
                throw new MicroAsmException($"Symbol not found '{symbol}'", line, lineNumber, _sourceFile);
            }

            if (_microOps.Contains(symbol))
            {
                _uOpsUsed.Add(symbol);
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
            var parts = line.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

        private void ParseCodeLinePass0(string line, int lineNumber)
        {
            if (line.StartsWith(LABEL))
            {
                var labelParts = line.Split(SPLIT_CHARS,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var symbol = labelParts[1];
                try
                {
                    var addressWord = new byte[_uCodeWordSizeBytes];
                    for (int addrB = 0; addrB < _uCodeAddrBytes; addrB++)
                    {
                        addressWord[addrB] = (byte)((_romAddress >> (8 * addrB)) & 0xFF);
                    }
                    for (var offset = _uCodeAddrBytes; offset < _uCodeWordSizeBytes; offset++)
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
            logLine += $"{CreateByteArrayString(_ROM[byteRomAddress..(byteRomAddress + _uCodeWordSizeBytes)])}";
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
                    var codeParts = line.Split(CODE_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var value = new byte[_uCodeWordSizeBytes];
                    foreach (var codePart in codeParts)
                    {
                        Or(ResolveSymbol(codePart, line, lineNumber), value);
                    }

                    int byteRomAddress = _romAddress * _uCodeWordSizeBytes;
                    var outputLogByteArray = new byte[_uCodeWordSizeBytes];
                    for (var offset = 0; offset < _uCodeWordSizeBytes; offset++)
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
            int numROMFiles = (_compositeROMSizeBytes + _individualROMSizeBytes - 1) / _individualROMSizeBytes;
            var writers = new BinaryWriter[numROMFiles];
            for (var romIndex = 0; romIndex < numROMFiles; romIndex++)
            {
                writers[romIndex] = new BinaryWriter(File.Open($"{romFile}-{romIndex}.bin", FileMode.Create));
            }

            int byteIndex = 0;
            while (byteIndex < _compositeROMSizeBytes)
            {
                for (var romIndex = 0; romIndex < numROMFiles && byteIndex < _compositeROMSizeBytes; romIndex++)
                {
                    for (int byteInRom = 0; byteInRom < _uCodeROMWordSizeBytes; byteInRom++)
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
                        for (var b = 0; b < _uCodeAddrBytes; b++)
                        {
                            if (b != 0)
                            {
                                addr >>= 8;
                            }
                            writer.Write((byte)(addr & 0xFF));
                        }
                    }
                    else
                    {
                        for (var b = 0; b < _uCodeAddrBytes; b++)
                        {
                            writer.Write((byte)0xFF); // Flags an error condition
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    throw new MicroAsmException($"The symbol decoder '{symbol}' is not implemented in the uCode");
                }
            }
            writer.Close();
        }
    }
}
