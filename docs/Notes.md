# Microinstruction Format

Microinstructions are 48 bits in length with the following format:

```
0xSSSSSSAAAA
```

Where `S` bytes are control signals, and `A` bytes are the target address for a microcode `jmp` or `jsr` operations.

Microinstructions are converted to little-endian order when written to the output ROM file.

# TO DO

* Remove hardcoded length dependencies from microasm.  *Done but needs testing*
* Reorder control lines into more logical groupings
* Consider moving memory decoding logic into a sub-circuit
* Split PC out to hi and lo and modify signal lines appropriately
* Add jump operations
* Add ALU
* Work out how to handle ALU flags
* Investigate using multiple phases of clocks to reduce execution cycles
* Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions

