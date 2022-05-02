using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

// All values are translated to Little Endian order and written that way to the output binary file
// Values are written to the console though in natural order
namespace microasm
{
    public class MS
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

        private static readonly Dictionary<string, byte[]> _symbols = new();
        private static readonly Dictionary<string, int> _opAddrs = new();
        private static readonly List<string> _outputLog = new();

        private static int _romAddress = 0;

        private static byte ParseBinaryByteString(string valueString)
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

        private static string ByteToBitString(byte value)
        {
            return Convert.ToString(value, 2).PadLeft(8, '0');
        }

        // Converts from little-endian to natural order
        private static void DumpSymbols()
        {
            Console.WriteLine("Symbol Table");
            Console.WriteLine("------------\n");

            _symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
            {
                Console.Write($"{k,-30}");
                var value = _symbols[k];
                for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
                {
                    Console.Write(ByteToBitString(value[byteIndex]).Replace("0", "."));
                    if (byteIndex != 0)
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            });
        }

        private static void DumpOpsAddrs()
        {
            Console.WriteLine("uAddress of Operations");
            Console.WriteLine("----------------------\n");

            _opAddrs.Keys.OrderBy(k => k).ToList().ForEach(k =>
            {
                Console.Write($"{k,-30}");
                Console.WriteLine($"{_opAddrs[k]:X}");
            });
        }

        private static void DumpOutputLog()
        {
            Console.WriteLine("Resolved Source");
            Console.WriteLine("---------------\n");

            _outputLog.ForEach(l =>
            {
                Console.WriteLine(l);
            });
        }

        private static void ParseFlagsLine(string line, int lineNumber)
        {
            var flagsParts = line.Split(' ', '\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (flagsParts.Length != 2)
            {
                throw new MicroAsmException($"Flag lines must consist of two white-space separated values", line, lineNumber);
            }

            var symbolString = flagsParts[0];
            var valueString = flagsParts[1];

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
                _symbols.Add(symbolString, value);
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

        private static void ParseMCOpsLine(string line, int lineNumber)
        {
            var mcopsParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (mcopsParts.Length!=2)
            {
                throw new MicroAsmException($"'{SECTION_MCOPS}' entries must consist of two space separated parts",
                    line, lineNumber);
            }
             
            var symbol = mcopsParts[0];
            var valueString = mcopsParts[1];

            if (valueString.Length!=2)
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
            // Addesses 0001 oooo oooo iiii are OpCodes i
            // Addreses 0000 xxxx xxxx xxxx are othe routines
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

        private static void ParseUCOpsLine(string line, int lineNumber)
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
                try
                {
                    Or(_symbols[currentRhs], value);
                }
                catch (KeyNotFoundException)
                {
                    throw new MicroAsmException($"Symbol not found '{currentRhs}'", line, lineNumber);
                }
            }

            try
            {
                _symbols.Add(symbol, value);
            }
            catch (ArgumentException)
            {
                throw new MicroAsmException($"Duplicate symbol '{symbol}'", line, lineNumber);
            }
        }

        private static void ParseCodeLine(string line, int lineNumber, byte[] ROM, int pass)
        {
            if (pass == 0)
            {
                if (line.StartsWith(LABEL))
                {
                    var labelParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var symbol = labelParts[1];
                    try
                    {
                        // !! Assumes word length
                        _symbols.Add(symbol, new byte[] { (byte)(_romAddress & 0xFF), (byte)((_romAddress >> 8) & 0xFF), 0, 0 });
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
            else
            { // pass 1
                if (!line.StartsWith(LABEL))
                {
                    if (line.StartsWith(OPCODE))
                    {
                        var opCodeParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (opCodeParts.Length!=2)
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
                            // catch missing symbol
                            Or(_symbols[codePart], value);
                        }

                        for (var offset = 0; offset < 4; offset++)
                        {
                            ROM[_romAddress * 4 + offset] = value[offset];
                        }

                        string opLine = $"{_romAddress:X4}\t{line,-30}";
                        for (var offset = 3; offset >= 0; offset--)
                        {
                            opLine += (ByteToBitString(ROM[_romAddress * 4 + offset]));
                            if (offset != 0)
                            {
                                opLine += "-";
                            }
                        }
                        _outputLog.Add(opLine);
                        _romAddress++;
                    }
                }
                else
                {
                    _outputLog.Add(line);
                }
            }      
        }

        private static void ParseLine(string line, Section section, int lineNumber, byte[] ROM, int pass)
        {
            switch (section)
            {
                case Section.Flags:
                    if (pass == 0)
                    {
                        ParseFlagsLine(line, lineNumber);
                    }
                    break;

                case Section.UCOps:
                    if (pass == 0)
                    {
                        ParseUCOpsLine(line, lineNumber);
                    }
                    break;

                case Section.MCOps:
                    if (pass == 0)
                    {
                        ParseMCOpsLine(line, lineNumber);
                    }
                    break;

                case Section.Code:
                    ParseCodeLine(line, lineNumber, ROM, pass);
                    break;
            }
        }

        private static void WriteFile(byte[] ROM, string filename)
        {
            var binWriter = new BinaryWriter(File.Open(filename, FileMode.Create));

            for (var index = 0; index < ROM.Length; index++)
            {
                binWriter.Write(ROM[index]);
            }

            binWriter.Close();
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("MicroAsm");
            Console.WriteLine("--------");
            Console.WriteLine("");

            var inputFile = args[0];
            var outputFile = args[1];

            Console.WriteLine($"Input file: '{inputFile}'");
            Console.WriteLine($"Output file: '{outputFile}'");
            Console.WriteLine();

            byte[] ROM = new byte[ROM_SIZE_BYTES];

            var currentSection = Section.None;

            try
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    int lineNumber = 1;
                    _romAddress = 0;
                    foreach (var line in File.ReadLines(inputFile))
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
                                        ParseLine(trimmedLine, currentSection, lineNumber, ROM, pass);
                                    }
                                    else
                                    {
                                        throw new Exception($"Directive outside of section at line {lineNumber}");
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (pass == 1)
                            {
                                if (trimmedLine.Length > 0)
                                {
                                    _outputLog.Add(trimmedLine);
                                }
                            }
                        }

                        lineNumber++;
                    }
                }
            
                WriteFile(ROM, outputFile);

                DumpSymbols();
                Console.WriteLine();
                DumpOpsAddrs();
                Console.WriteLine();
                DumpOutputLog();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}