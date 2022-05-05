# TO DO

* ~~Remove hardcoded length dependencies from microasm.~~ DONE
* Reorder control lines into more logical groupings
* Add HALT switch
* Add Reset switch - and initialize via vector
* Consider using consistent addressing mode bit patterns together with phased uCode execution to automatically have correct addressing code executed and avoid the use subroutines.   This would reduce both execution cycles and code complexity.
* ~~Sort out an machine code assembler to more easily create test code (dasm?)~~ DONE
* Consider moving memory decoding logic into a sub-circuit
* Split PC out to hi and lo and modify signal lines appropriately
* ~~Add jump operation~~ DONE
* Add ALU
* Work out how to handle ALU flags
* Investigate using multiple phases of clocks to reduce execution cycles
* Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions
  