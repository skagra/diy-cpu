# Microinstruction Format

Microinstructions are 48 bits in length with the following format:

```
0xSSSAA
```

Where `S` bytes are control signals, and `A` bytes are the target address for a microcode `jmp` or `jsr` operations.

Microinstructions are converted to little-endian order when written to the output ROM file by `microasm`.
# References

* 6502 Instruction set - good analysis of bit patterns for addressing modes:
https://link.springer.com/content/pdf/bbm%3A978-1-349-07360-3%2F1.pdf

* Nice indexed list of opcodes:
http://www.6502.org/tutorials/6502opcodes.html

* Assembler - DASM https://dasm-assembler.github.io/