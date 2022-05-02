using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

public class MicroAsmException : Exception
{
    public MicroAsmException(string message, string line, int lineNumber) : base($"Error {message} at line {lineNumber}, '{line}' ")
    {
    }
}

// Little Endian
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

    private static byte[] BytePowers = { 128, 64, 32, 16, 8, 4, 2, 1 };

    private static Dictionary<string, byte[]> symbols = new Dictionary<string, byte[]>();
    private static Dictionary<string, int> OpAddrs = new Dictionary<string, int>();

    // TODO - Assumes 8 bit string without checking
    private static byte ParseFlagValue(string valueString)
    {
        byte result = 0;

        for (int valueStringIndex = 0; valueStringIndex < 8; valueStringIndex++)
        {
            if (valueString[valueStringIndex] == '1')
            {
                result |= BytePowers[valueStringIndex];
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
        symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
        {
            Console.Write($"{k,-30}");
            var value = symbols[k];
            for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
            {
                Console.Write(ByteToBitString(value[byteIndex]));
                if (byteIndex != 0)
                {
                    Console.Write("-");
                }
            }
            Console.WriteLine();
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
            throw new MicroAsmException($"Flag value must be {FLAGS_SIZE_IN_BYTES * 8} characters long", line, lineNumber);
        }

        var value = new byte[WORD_SIZE_IN_BYTES];
        for (var valByteIndex = 0; valByteIndex < FLAGS_SIZE_IN_BYTES; valByteIndex++)
        {
            value[WORD_SIZE_IN_BYTES - valByteIndex - 1] = ParseFlagValue(valueString.Substring(valByteIndex * 8, 8));
        }

        try
        {
            symbols.Add(symbolString, value);
        }
        catch (ArgumentException)
        {
            throw new MicroAsmException($"Symbol already seen '{symbolString}'", line, lineNumber);
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

        // TODO check there are two!

        var symbol = mcopsParts[0];
        byte value = byte.Parse(mcopsParts[1], System.Globalization.NumberStyles.HexNumber);

        // TODO check parse works // check in range (2 chars)
        UInt16 mappedAddress = (UInt16)(value << 4 | 0b1000000000000); // 16 words for each instruction, starting a 4096 base
        OpAddrs.Add(symbol, mappedAddress);

        // Check symbol not already seen
        Console.WriteLine(value);
    }

    private static void ParseUCOpsLine(string line, int lineNumber)
    {
        var ucopsParts = line.Split(new char[] { ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (ucopsParts.Length < 2)
        {
            throw new MicroAsmException($"Each .ucops line must contain a symbol followed by one or more values", line, lineNumber);
        }

        var symbol = ucopsParts[0];
        var value = new byte[WORD_SIZE_IN_BYTES];

        for (var rhsIndex = 1; rhsIndex < ucopsParts.Length; rhsIndex++)
        {
            var currentRhs = ucopsParts[rhsIndex];
            try
            {
                Or(symbols[currentRhs], value);
            }
            catch (KeyNotFoundException)
            {
                throw new MicroAsmException($"Symbol not found '{currentRhs}'", line, lineNumber);
            }
        }

        try
        {
            symbols.Add(symbol, value);
        }
        catch (ArgumentException)
        {
            throw new MicroAsmException($"Symbol already seen '{symbol}'", line, lineNumber);
        }
    }

    private static int uPC = 0;

    private static void ParseCodeLine(string line, int lineNumber, byte[] ROM, int pass)
    {
        if (pass == 0)
        {
            if (line.StartsWith(".label"))
            {
                var labelParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                //TODO check number and already seen
                // TODO assumes world length

                symbols.Add(labelParts[1], new byte[] { (byte)(uPC & 0xFF), (byte)((uPC >> 8) & 0xFF), 0, 0 });

                Console.WriteLine("ADDING Label: " + uPC);
            }
            else
            {
                uPC += 1;
            }
        }
        else
        {
            if (pass == 1)
            {
                if (!line.StartsWith(".label"))
                {
                    if (line.StartsWith(".origin"))
                    {
                        var originParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        // Check there are two! // check it is found;
                        uPC = OpAddrs[originParts[1]];
                    }
                    else
                    {
                        var codeParts = line.Split(new char[] { ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var value = new byte[WORD_SIZE_IN_BYTES];
                        foreach (var codePart in codeParts)
                        {
                            // catch missing symbol
                            Or(symbols[codePart], value);
                        }
                        for (var offset = 0; offset < 4; offset++)
                        {
                            Console.WriteLine($"Setting ROM byte at {uPC} with value {ByteToBitString(value[offset])}");
                            ROM[uPC * 4 + offset] = value[offset];
                        }
                        symbols.Add(uPC.ToString(), value);
                        uPC++;
                    }
                }
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

    private static void WriteFile(byte[] ROM)
    {
        const string filename = "xxx.bin";

        var binWriter = new BinaryWriter(File.Open(filename, FileMode.Create));

        for (var index = 0; index < ROM.Length; index++)
        {
            binWriter.Write(ROM[index]);
            //Console.WriteLine($"Writing byte at {index} with value {ByteToBitString(ROM[index])}");
        }

        binWriter.Close();
    }

    public static void Main(string[] args)
    {
        byte[] ROM = new byte[ROM_SIZE_BYTES];

        var currentSection = Section.None;

        for (int pass = 0; pass < 2; pass++)
        {
            int lineNumber = 1;
            uPC = 0;
            foreach (var line in File.ReadLines(@"..\ucode\mc-rom.txt"))
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

                lineNumber++;
            }
        }

        WriteFile(ROM);

        DumpSymbols();
    }
}