# Microinstruction Format

Microinstructions are 48 bits in length with the following format:

```
0xSSSAA
```

Where `S` bytes are control signals, and `A` bytes are the target address for a microcode `jmp` or `jsr` operations.

Microinstructions are converted to little-endian order when written to the output ROM file by `microasm`.

# TO DO

* ~~Remove hardcoded length dependencies from microasm.~~ DONE
* Reorder control lines into more logical groupings
* Add HALT switch
* Add Reset switch - and initialize via vector
* Consider using consistent addressing mode bit patterns together with phased uCode execution to automatically have correct addressing code executed and avoid the use subroutines.   This would reduce both execution cycles and code complexity.
* ~~Sort out an machine code assembler to more easily create test code (dasm?)~~ DONE
* Consider moving memory decoding logic into a sub-circuit
* Split PC out to hi and lo and modify signal lines appropriately
* Add jump operations
* Add ALU
* Work out how to handle ALU flags
* Investigate using multiple phases of clocks to reduce execution cycles
* Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions

# References

* 6502 Instruction set - good analysis of bit patterns for addressing modes:
https://link.springer.com/content/pdf/bbm%3A978-1-349-07360-3%2F1.pdf

* Nice indexed list of opcodes:
http://www.6502.org/tutorials/6502opcodes.html

* Assembler - DASM https://dasm-assembler.github.io/