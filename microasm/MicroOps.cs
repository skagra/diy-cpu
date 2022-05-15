
namespace microasm
{
    using static MicroAsmConstants;
    using static MicroAsmFormatting;

    public class MicroOps
    {
        private readonly Dictionary<string, byte[]> _ucopsSymbols = new();

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
            return _ucopsSymbols.TryGetValue(symbol, out result);
        }

        private static void Or(byte[] source, byte[] dest)
        {
            for (var i = 0; i < source.Length; i++)
            {
                dest[i] |= source[i];
            }
        }

        private void ParseUCOpsLine(string line, int lineNumber)
        {
            var ucopsParts = line.Split(new char[] { ' ', '\t', '|' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (ucopsParts.Length < 2)
            {
                throw new MicroAsmException(
                    $"Each entry must contain a symbol followed by one or more values",
                    line, lineNumber, _sourceFile
                );
            }

            var symbol = ucopsParts[0];
            var value = new byte[WORD_SIZE_IN_BYTES];

            for (var rhsIndex = 1; rhsIndex < ucopsParts.Length; rhsIndex++)
            {
                var currentRhs = ucopsParts[rhsIndex];

                byte[] resolvedSymbol;
                if (_uCtrl.TryGetValue(currentRhs, out resolvedSymbol))
                {
                    Or(resolvedSymbol, value);
                }
                else
                {
                    throw new MicroAsmException($"Symbol not found '{currentRhs}'", line, lineNumber, _sourceFile);
                }
            }

            try
            {
                _ucopsSymbols.Add(symbol, value);
            }
            catch (ArgumentException)
            {
                throw new MicroAsmException($"Duplicate symbol '{symbol}'",
                    line, lineNumber, _sourceFile);
            }
        }

        private void Parse()
        {
            int lineNumber = 1;

            foreach (var line in File.ReadLines(_sourceFile))
            {
                var trimmedLine = line.Trim();

                // Skip empty lines and comment lies
                if (trimmedLine.Length > 0 && !trimmedLine.StartsWith(COMMENT_CHARACTERS))
                {
                    ParseUCOpsLine(trimmedLine, lineNumber);
                }
                lineNumber++;
            }
        }

        public override string ToString()
        {
            return DumpSymbols(_ucopsSymbols);
        }
    }
}
