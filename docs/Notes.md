# Microinstruction Format

Microinstructions are 72 bits in length with the following format:

```
0xSSSSSSSSAA
```

Where `S` bytes are control signals, and `A` bytes are the target address for a microcode `uJMP`, `uZJMP`, `uNJMP`, `uVJMP`, `uCJMP` or `uJSR` operation.

Microinstructions are converted to little-endian order when written to the output ROM file by `microasm`.

```
Natural order                    uROM (little endian) order
S9 S8 S7 S6 S5 S4 S3 S2 A1 A0 => AO A1 S2 S3 S4 S5 S6 S7 S8 S9 
```

# Addressing Modes

| Mode | Example | Description |
|------|---------|-------------|
| Accumulator | `ASL A` | Implicitly targets the accumulator 
| Absolute | `ADC $4400` | Argument gives the address of the operand. 
| Absolute, X | `ADC $4400,X` | Argument is added to X (with carry) to give the two byte address of the operand. 
| Absolute, Y | `ADC $4400,Y` | Argument is added to Y (with carry) to give the two byte address of the operand. 
| Immediate | `ADC #$44`| Argument is the operand 
| Implied | `TSX` | No operand |
| Indirect | `JMP ($5597)` | Argument gives the address of a two byte value used as the target of a JMP operation. **Note** that the calculation of the address of the second indirected byte is done without carry!  So the operation will not give the expected result if argument refers to the last byte in a page. Used only for JMP. 
| X, Indirect | `ADC ($44,X)` | The zero page argument added to X gives the zero page address of the two byte value operand.  Addition wraps to zero-page.  The argument is always 0 page. 
| Indirect, Y | `ADC ($44),Y` | The argument points to a two byte zero page value which is retrieved and added to Y to give the operand. 
| Relative | `BPL $50` | The operand is an 8 bit signed offset from the PC. 
| Zero page | `ADC $44` | The argument is the zero-page address of the operand 
| Zero page, X | `ADC $44,X` | The zero page argument is added to X to give the zero page address of the operand.  Addition wraps to zero page. 
| Zero page, Y | `STA $00,Y` | The zero page argument is added to Y to give the zero page address of the operand.  Addition wraps to zero page. 

# References

* 6502 Instruction set - good analysis of bit patterns for addressing modes: https://link.springer.com/content/pdf/bbm%3A978-1-349-07360-3%2F1.pdf
* Nice indexed list of opcodes: http://www.6502.org/tutorials/6502opcodes.html
* Table of opcodes with some grouping breakdown https://llx.com/Neil/a2/opcodes.html
* Assembler - DASM https://dasm-assembler.github.io/
* Someone else has done something similar: https://c74project.com/
* Nice summary table of Op Codes https://masswerk.at/6502/6502_instruction_set.html
* 6502 programming manual - https://archive.org/details/mos_microcomputers_programming_manual
* Very nice summary - https://xotmatrix.github.io/6502/6502-instruction-set.html
* Instruction layout - https://www.masswerk.at/6502/6502_instruction_set.html#layout