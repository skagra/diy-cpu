
namespace microasm
{
    using System.Linq;
    using static MicroAsmConstants;
    using static MicroAsmFormatting;

    public class MicroOps
    {
        private readonly Dictionary<string, byte[]> _resolvedSymbols = new();
        private readonly HashSet<string> _seenSymbols = new();

        private readonly MicroCtrl _uCtrl;

        private readonly string _sourceFile;

        public MicroOps(MicroCtrl uCtrl, string sourceFile)
        {
            _uCtrl = uCtrl;
            _sourceFile = sourceFile;
            Parse();
        }

        public bool TryGetValue(string symbol, out byte[] result)
        {
            return _resolvedSymbols.TryGetValue(symbol, out result);
        }

        private static void Or(byte[] source, byte[] dest)
        {
            for (var i = 0; i < source.Length; i++)
            {
                dest[i] |= source[i];
            }
        }

        private void ParseUCOpsLine(string line, int lineNumber, int pass)
        {
            var ucopsParts = line.Split(new char[] { ' ', '\t', '|' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (ucopsParts.Length < 2)
            {
                throw new MicroAsmException($"Each entry must contain a symbol followed by one or more values",
                    line, lineNumber, _sourceFile);
            }

            var symbol = ucopsParts[0];
            var value = new byte[WORD_SIZE_IN_BYTES];

            // Have we seen the symbol already on the first pass?
            if (pass == 0 && _seenSymbols.Contains(symbol))
            {
                // Must be a duplicate
                throw new MicroAsmException($"Duplicate symbol '{symbol}'", line, lineNumber, _sourceFile);
            }
            _seenSymbols.Add(symbol);

            // Is the symbol already resolved?
            if (!_resolvedSymbols.ContainsKey(symbol))
            {
                bool resolved = true;
                string currentRhs = null;
                for (var rhsIndex = 1; rhsIndex < ucopsParts.Length; rhsIndex++)
                {
                    currentRhs = ucopsParts[rhsIndex];

                    byte[] resolvedSymbol;
                    if (TryGetValue(currentRhs, out resolvedSymbol) ||
                        _uCtrl.TryGetValue(currentRhs, out resolvedSymbol))
                    {
                        Or(resolvedSymbol, value);
                    }
                    else
                    {
                        resolved = false;
                        break;
                    }
                }

                if (resolved)
                {
                    _resolvedSymbols.Add(symbol, value);
                }
                else if (pass == 1)
                {
                    // Second pass and still not resolved
                    throw new MicroAsmException($"Symbol not found '{currentRhs}'", line, lineNumber, _sourceFile);
                }
            }
        }

        private void Parse()
        {
            for (int pass = 0; pass < 2; pass++)
            {
                int lineNumber = 1;
                foreach (var line in File.ReadLines(_sourceFile))
                {
                    var trimmedLine = TrimAndStripComments(line);

                    // Skip empty lines and comment lies
                    if (trimmedLine.Length > 0)
                    {
                        ParseUCOpsLine(trimmedLine, lineNumber, pass);
                    }
                    lineNumber++;
                }
            }
        }

        public override string ToString()
        {
            return DumpSymbols(_resolvedSymbols);
        }
    }
}
