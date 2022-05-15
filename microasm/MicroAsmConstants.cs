namespace microasm
{
    public static class MicroAsmConstants
    {
        // Characters which introduce a comment in the uCode file
        public const string COMMENT_CHARACTERS = "//";

        // Number of bytes for control signals
        public const int FLAGS_SIZE_IN_BYTES = 8;

        // Size of each uOpCode - FLAGS_SIZE_IN_BYTES+2 for 16 bit uROM address
        public const int WORD_SIZE_IN_BYTES = FLAGS_SIZE_IN_BYTES + 2;
    }
}
