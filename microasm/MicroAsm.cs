using System.Text;

namespace microasm
{
    public class MicroAsm
    {
        private enum Section { None, Flags, UCOps, MCOps, Code }

        private const int WORD_SIZE_IN_BYTES = 4;

        private const int WORD_COUNT = 0x10000; //32; // 0x10000;
        private const int FLAGS_SIZE_IN_BYTES = 2;

        private const int ROM_SIZE_BYTES = WORD_SIZE_IN_BYTES * WORD_COUNT;

        private const string COMMENT_CHARS = "//";

        private const string SECTION_FLAGS = ".flags";
        private const string SECTION_UCOPS = ".ucops";
        private const string SECTION_MCOPS = ".mcops";
        private const string SECTION_CODE = ".code";

        private const string OPCODE = ".opcode";
        private const string LABEL = ".label";

        private static readonly byte[] BytePowers = { 128, 64, 32, 16, 8, 4, 2, 1 };

        private readonly Dictionary<string, byte[]> _flagSymbols = new();
        private readonly Dictionary<string, byte[]> _ucopsSymbols = new();
        private readonly Dictionary<string, byte[]> _labelSymbols = new();
        private readonly Dictionary<string, byte[]> _codeSymbols = new();

        private readonly Dictionary<string, int> _opAddrs = new();
        private readonly List<string> _outputLog = new();

        private readonly byte[] _ROM = new byte[ROM_SIZE_BYTES];

        private int _romAddress = 0;
        private Section currentSection = Section.None;

        public MicroAsm(string sourceFile)
        {
            Parse(sourceFile);
        }

        private byte[] ResolveSymbol(string symbol, string line, int lineNumber)
        {
            byte[] result;

            if (!(_flagSymbols.TryGetValue(symbol, value: out result) ||
               _ucopsSymbols.TryGetValue(symbol, value: out result) ||
               _labelSymbols.TryGetValue(symbol, value: out result) ||
               _codeSymbols.TryGetValue(symbol, value: out result)))
            {
                throw new MicroAsmException($"Symbol not found '{symbol}'", line, lineNumber);
            }

            return result;
        }

        private byte ParseBinaryByteString(string valueString)
        {
            if (valueString.Length != 8)
            {
                throw new MicroAsmException($"Invalid binary byte string, the string is not 8 characters long '{valueString}'");
            }

            byte result = 0;
            for (int valueStringIndex = 0; valueStringIndex < 8; valueStringIndex++)
            {
                var valueChar = valueString[valueStringIndex];
                switch (valueChar)
                {
                    case '1':
                    case 'x':
                    case 'X':
                        result |= BytePowers[valueStringIndex];
                        break;
                    case '.':
                    case '-':
                    case '_':
                    case '0':
                        break;
                    default:
                        throw new MicroAsmException($"Invalid character in binary string '{valueChar}'");
                }
            }

            return result;
        }

        private string ByteToBitString(byte value)
        {
            return Convert.ToString(value, 2).PadLeft(8, '0');
        }

        private string CreateByteArrayString(byte[] value)
        {
            var result = new StringBuilder();

            for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
            {
                result.Append(ByteToBitString(value[byteIndex]).Replace("0", "."));
                if (byteIndex != 0)
                {
                    result.Append(" ");
                }
            }

            result.Append($" {value[3]:X2} {value[2]:X2} {value[1]:X2} {value[0]:X2}");

            return result.ToString();
        }

        // Converts from little-endian to natural order
        private void DumpSymbols(Dictionary<string, byte[]> symbols)
        {
            symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
            {
                var value = symbols[k];
                Console.WriteLine($"{k,-40} {CreateByteArrayString(value)}");
            });
        }

        public void DumpFlagSymbols()
        {
            Console.WriteLine("Flags Symbol Table");
            Console.WriteLine("------------------\n");
            DumpSymbols(_flagSymbols);
        }

        public void DumpUCopsSymbols()
        {
            Console.WriteLine("UCOPS Symbol Table");
            Console.WriteLine("------------------\n");
            DumpSymbols(_ucopsSymbols);
        }

        public void DumpLabelSymbols()
        {
            Console.WriteLine("Labels Symbol Table");
            Console.WriteLine("------------------\n");
            DumpSymbols(_labelSymbols);
        }

        public void DumpOpsAddrs()
        {
            Console.WriteLine("uAddress of Operations");
            Console.WriteLine("----------------------\n");

            _opAddrs.Keys.OrderBy(k => k).ToList().ForEach(k =>
            {
                Console.WriteLine($"{k,-40}{ _opAddrs[k]:X4}");
            });
        }

        public void DumpOutputLog()
        {
            Console.WriteLine("Resolved Source");
            Console.WriteLine("---------------\n");

            _outputLog.ForEach(l => Console.WriteLine(l));
        }

        private void ParseFlagsLine(string line, int lineNumber)
        {
            var flagsParts = line.Split(' ', '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (flagsParts.Length < 2)
            {
                throw new MicroAsmException($"Flag lines must consist of two white-space separated values", line, lineNumber);
            }

            var symbolString = flagsParts[0];
            var valueString = string.Join(' ', flagsParts[1..]).Replace(" ", "");

            if (valueString.Length != FLAGS_SIZE_IN_BYTES * 8)
            {
                throw new MicroAsmException($"Flag value must be '{FLAGS_SIZE_IN_BYTES * 8}' characters long", line, lineNumber);
            }

            var value = new byte[WORD_SIZE_IN_BYTES];
            for (var valByteIndex = 0; valByteIndex < FLAGS_SIZE_IN_BYTES; valByteIndex++)
            {
                value[WORD_SIZE_IN_BYTES - valByteIndex - 1] = ParseBinaryByteString(valueString.Substring(valByteIndex * 8, 8));
            }

            try
            {
                _flagSymbols.Add(symbolString, value);
            }
            catch (ArgumentException)
            {
                throw new MicroAsmException($"Duplicate symbol '{symbolString}'", line, lineNumber);
            }
        }

        private static void Or(byte[] source, byte[] dest)
        {
            for (var i = 0; i < source.Length; i++)
            {
                dest[i] |= source[i];
            }
        }

        private void ParseMCOpsLine(string line, int lineNumber)
        {
            var mcopsParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (mcopsParts.Length != 2)
            {
                throw new MicroAsmException($"'{SECTION_MCOPS}' entries must consist of two space separated parts",
                    line, lineNumber);
            }

            var symbol = mcopsParts[0];
            var valueString = mcopsParts[1];

            if (valueString.Length != 2)
            {
                throw new MicroAsmException($"Values in '{SECTION_MCOPS}' entries must consist of two hex characters",
                    line, lineNumber);
            }

            byte value;
            try
            {
                value = byte.Parse(valueString, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                throw new MicroAsmException($"Failed to parse '{SECTION_MCOPS}' value '{valueString}'",
                    line, lineNumber);
            }

            // 16 words for each instruction, starting a 4096 base
            // Addresses 0001 oooo oooo iiii are OpCodes i
            // Addresses 0000 xxxx xxxx xxxx are othe routines
            UInt16 mappedAddress = (UInt16)(value << 4 | 0b1000000000000);
            try
            {
                _opAddrs.Add(symbol, mappedAddress);
            }
            catch (ArgumentException)
            {
                throw new MicroAsmException($"Duplicate symbol '{symbol}'",
                    line, lineNumber);
            }
        }

        private void ParseUCOpsLine(string line, int lineNumber)
        {
            var ucopsParts = line.Split(new char[] { ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (ucopsParts.Length < 2)
            {
                throw new MicroAsmException($"Each '{SECTION_UCOPS}' entry must contain a symbol followed by one or more values", line, lineNumber);
            }

            var symbol = ucopsParts[0];
            var value = new byte[WORD_SIZE_IN_BYTES];

            for (var rhsIndex = 1; rhsIndex < ucopsParts.Length; rhsIndex++)
            {
                var currentRhs = ucopsParts[rhsIndex];
                Or(ResolveSymbol(currentRhs, line, lineNumber), value);
            }

            try
            {
                _ucopsSymbols.Add(symbol, value);
            }
            catch (ArgumentException)
            {
                throw new MicroAsmException($"Duplicate symbol '{symbol}'", line, lineNumber);
            }
        }

        private void ParseCodeLinePass0(string line, int lineNumber)
        {

            if (line.StartsWith(LABEL))
            {
                var labelParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var symbol = labelParts[1];
                try
                {
                    // !! Assumes word length
                    _labelSymbols.Add(symbol, new byte[] { (byte)(_romAddress & 0xFF), (byte)((_romAddress >> 8) & 0xFF), 0, 0 });
                }
                catch
                {
                    throw new MicroAsmException($"Duplicate symbol '{symbol}'", line, lineNumber);
                }
            }
            else
            {
                if (!line.StartsWith(OPCODE))
                {
                    _romAddress += 1;
                }
            }

        }

        private void ParseCodeLinePass1(string line, int lineNumber)
        {

            if (!line.StartsWith(LABEL))
            {
                if (line.StartsWith(OPCODE))
                {
                    var opCodeParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (opCodeParts.Length != 2)
                    {
                        throw new MicroAsmException($"'{OPCODE}' lines must consist of two space separated vales", line, lineNumber);
                    }

                    try
                    {
                        _romAddress = _opAddrs[opCodeParts[1]];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new MicroAsmException($"'{OPCODE}' value not found", line, lineNumber);
                    }
                    _outputLog.Add($"{OPCODE} {opCodeParts[1]}");
                }
                else
                {
                    var codeParts = line.Split(new char[] { ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var value = new byte[WORD_SIZE_IN_BYTES];
                    foreach (var codePart in codeParts)
                    {
                        Or(ResolveSymbol(codePart, line, lineNumber), value);
                    }

                    for (var offset = 0; offset < 4; offset++)
                    {
                        _ROM[_romAddress * 4 + offset] = value[offset];
                    }

                    int byteRomAddress = _romAddress * 4;
                    _outputLog.Add($"{_romAddress:X4}\t{line,-30} {CreateByteArrayString(new byte[] { _ROM[byteRomAddress], _ROM[byteRomAddress + 1], _ROM[byteRomAddress + 2], _ROM[byteRomAddress + 3] })}");
                    _romAddress++;
                }
            }
            else
            {
                _outputLog.Add(line);
            }
        }

        private void ParseLine(string line, Section section, int lineNumber, int pass)
        {
            if (pass == 0)
            {
                switch (section)
                {
                    case Section.Flags:
                        ParseFlagsLine(line, lineNumber);
                        break;

                    case Section.UCOps:
                        ParseUCOpsLine(line, lineNumber);
                        break;

                    case Section.MCOps:
                        ParseMCOpsLine(line, lineNumber);
                        break;

                    case Section.Code:
                        ParseCodeLinePass0(line, lineNumber);
                        break;
                }
            }
            else if (section == Section.Code)
            {
                ParseCodeLinePass1(line, lineNumber);
            }

        }

        public void WriteROMFile(string romFile)
        {
            var binWriter = new BinaryWriter(File.Open(romFile, FileMode.Create));

            for (var index = 0; index < _ROM.Length; index++)
            {
                binWriter.Write(_ROM[index]);
            }

            binWriter.Close();
        }

        private void Parse(string sourceFile)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                int lineNumber = 1;
                _romAddress = 0;
                foreach (var line in File.ReadLines(sourceFile))
                {
                    var trimmedLine = line.Trim();

                    // Skip empty lines and comment lies
                    if (trimmedLine.Length > 0 && !trimmedLine.StartsWith(COMMENT_CHARS))
                    {
                        switch (trimmedLine)
                        {
                            case SECTION_FLAGS:
                                currentSection = Section.Flags;
                                break;

                            case SECTION_UCOPS:
                                currentSection = Section.UCOps;
                                break;

                            case SECTION_MCOPS:
                                currentSection = Section.MCOps;
                                break;

                            case SECTION_CODE:
                                currentSection = Section.Code;
                                break;

                            default:
                                if (currentSection != Section.None)
                                {
                                    ParseLine(trimmedLine, currentSection, lineNumber, pass);
                                }
                                else
                                {
                                    throw new MicroAsmException($"Directive outside of section", trimmedLine, lineNumber);
                                }
                                break;
                        }
                    }
                    lineNumber++;
                }
            }
        }
    }
}
