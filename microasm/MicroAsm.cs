using System.Text;

namespace microasm
{
   public class MicroAsm
   {
      // Idenfies the section of the uCode file being parsed
      private enum Section { None, Flags, UCOps, Code }

      // Number of bytes for control signals
      private const int FLAGS_SIZE_IN_BYTES = 8;

      // Size of each uOpCode - FLAGS_SIZE_IN_BYTES+2 for 16 bit uROM address
      private const int WORD_SIZE_IN_BYTES = FLAGS_SIZE_IN_BYTES + 2;

      // Size of the output ROM in words
      private const int WORD_COUNT = 0x10000;

      // Size of the output ROM in bytes
      private const int TOTAL_ROM_SIZE_BYTES = WORD_SIZE_IN_BYTES * WORD_COUNT;

      // Width in bytes of data in each output ROM file
      private const int ROM_DATA_WIDTH_BYTES = 2;

      // Size of each ROM
      private const int ROM_SIZE_BYTES = ROM_DATA_WIDTH_BYTES * 65536;

      // Characters which introduce a comment in the uCode file
      private const string COMMENT_CHARS = "//";

      // Control flags section
      private const string SECTION_FLAGS = ".flags";

      // uInstructions as combinations of control flags
      private const string SECTION_UCOPS = ".ucops";

      // Definition of actual uCode
      private const string SECTION_CODE = ".code";

      // Identifies uCode implementing a machine code op
      private const string OPCODE = ".opcode";

      // Identifies uCode implementation a machine code addressing mode
      private const string MODE = ".mode";

      // Label a location so it may be referred to later
      private const string LABEL = ".label";

      // For efficient conversion of bit strings to byte values
      private static readonly byte[] BytePowers = { 128, 64, 32, 16, 8, 4, 2, 1 };

      // Symbols tables filled in as the uCode file is parsed then used to generate ROM output
      // Each symbol value is in output (little endian) order and is the full size of uCode word
      private readonly Dictionary<string, byte[]> _flagSymbols = new();
      private readonly Dictionary<string, byte[]> _ucopsSymbols = new();
      private readonly Dictionary<string, byte[]> _labelSymbols = new();
      private readonly Dictionary<string, byte[]> _codeSymbols = new();

      private readonly Dictionary<string, int> _opCodeRoutineAddresses = new();
      private readonly Dictionary<string, int> _modeRoutineAddresses = new();

      private readonly MappingRom _opCodeMappingROM;
      private readonly MappingRom _modeMappingROM;

      private readonly List<string> _outputLog = new();

      private readonly byte[] _ROM = new byte[TOTAL_ROM_SIZE_BYTES];

      private int _romAddress = 0;
      private Section currentSection = Section.None;

      public MicroAsm(MappingRom opCodeMappingROM, MappingRom modeMappingROM, string sourceFile)
      {
         _opCodeMappingROM = opCodeMappingROM;
         _modeMappingROM = modeMappingROM;
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
            result.Append(ByteToBitString(value[byteIndex]).Replace("0", ".") + " ");
         }

         result.Append("\t");

         for (var byteIndex = value.Length - 1; byteIndex >= 0; byteIndex--)
         {
            result.Append($"{value[byteIndex]:X2}");
            if (byteIndex != 0)
            {
               result.Append(" ");
            }
         }

         return result.ToString();
      }

      // Converts from little-endian to natural order
      private string DumpSymbols(Dictionary<string, byte[]> symbols)
      {
         var result = new StringBuilder();
         symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
         {
            var value = symbols[k];
            result.AppendLine($"{k,-40} {CreateByteArrayString(value)}");
         });
         return result.ToString();
      }

      private string DumpMap(Dictionary<string, int> symbols)
      {
         var result = new StringBuilder();
         symbols.Keys.OrderBy(k => k).ToList().ForEach(k =>
         {
            var value = symbols[k];
            result.AppendLine($"{k,-20} {value:X4}");
         });

         return result.ToString();
      }

      public string DumpFlagSymbols()
      {
         return DumpSymbols(_flagSymbols);
      }

      public string DumpUCopsSymbols()
      {
         return DumpSymbols(_ucopsSymbols);
      }

      public string DumpLabelSymbols()
      {
         return DumpSymbols(_labelSymbols);
      }

      public string DumpOutputLog()
      {
         return (string.Join('\n', _outputLog));
      }

      public string DumpModeMap()
      {
         return DumpMap(_modeRoutineAddresses);
      }

      public string DumpOpCodeMap()
      {
         return DumpMap(_opCodeRoutineAddresses);
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

      public int GetRomSizeWords()
      {
         return _romAddress;
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

      // TODO: SAME CODE TWICE FACTOR OUT
      private void ProcessOpCodeLine(string line, int lineNumber, int phase)
      {
         var opCodeParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
         if (opCodeParts.Length != 2)
         {
            throw new MicroAsmException($"'{OPCODE}' lines must consist of two space separated values", line, lineNumber);
         }

         if (phase == 0)
         {
            try
            {
               _opCodeRoutineAddresses[opCodeParts[1]] = _romAddress;
            }
            catch (KeyNotFoundException)
            {
               throw new MicroAsmException($"'{OPCODE}' value not found", line, lineNumber);
            }
         }
         else
         {
            _outputLog.Add($"\n{OPCODE} {opCodeParts[1]}");
         }
      }


      // TODO: SAME CODE TWICE FACTOR OUT
      private void ProcessAddrModeLine(string line, int lineNumber, int phase)
      {
         var opCodeParts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
         if (opCodeParts.Length != 2)
         {
            throw new MicroAsmException($"'{MODE}' lines must consist of two space separated values", line, lineNumber);
         }

         if (phase == 0)
         {
            try
            {
               _modeRoutineAddresses[opCodeParts[1]] = _romAddress;
            }
            catch (KeyNotFoundException)
            {
               throw new MicroAsmException($"'{MODE}' value not found", line, lineNumber);
            }
         }
         else
         {
            _outputLog.Add($"\n{MODE} {opCodeParts[1]}");
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
               throw new MicroAsmException($"Duplicate symbol '{symbol}'", line, lineNumber);
            }
         }
         else
         {
            if (line.StartsWith(OPCODE))
            {
               ProcessOpCodeLine(line, lineNumber, 0);
            }
            else
            if (line.StartsWith(MODE))
            {
               ProcessAddrModeLine(line, lineNumber, 0);
            }
            else
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
               ProcessOpCodeLine(line, lineNumber, 1);
            }
            else
            if (line.StartsWith(MODE))
            {
               ProcessAddrModeLine(line, lineNumber, 1);
            }
            else
            {
               var codeParts = line.Split(new char[] { ' ', '\t', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

               var logLine = $"{_romAddress:X4}    {line,-30}";
               if (line.Length > 28)
               {
                  logLine += $"\n{' ',38}";
               }
               logLine += $"{CreateByteArrayString(_ROM[byteRomAddress..(byteRomAddress + WORD_SIZE_IN_BYTES)])}";
               _outputLog.Add(logLine);

               _romAddress++;
            }
         }
         else
         {
            _outputLog.Add($"\n{line,-40}");
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

      // TODO: Not allowing number of bytes does not match rom size 
      public void WriteROMFile(string romFile)
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
            for (int romIndex = 0; romIndex < numROMFiles && byteIndex < TOTAL_ROM_SIZE_BYTES; romIndex++)
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

      public void WriteOpCodeMappingFile(string fileName)
      {
         WriteROMMAppingFile(_opCodeMappingROM, _opCodeRoutineAddresses, fileName);
      }

      public void WriteModeMappingFile(string fileName)
      {
         WriteROMMAppingFile(_modeMappingROM, _modeRoutineAddresses, fileName);
      }

      private void WriteROMMAppingFile(MappingRom mapping, Dictionary<string, int> _mappingAddresses, string romFile)
      {
         var writer = new BinaryWriter(File.Open(romFile, FileMode.Create));

         // TODO: HACKY
         for (var index = 0; index < 256; index++)
         {
            var resolvedMapping = mapping.ResolveIndex(index);
            if (resolvedMapping != null)
            {
               int addr = _mappingAddresses[resolvedMapping];
               writer.Write((byte)(addr & 0xFF));
               writer.Write((byte)((addr >> 8) & 0xFF));
            }
            else
            {
               writer.Write((byte)0xFF);
               writer.Write((byte)0xFF);
            }
         }
         writer.Close();
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
