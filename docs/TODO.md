# TO DO

* Reorder control lines into more logical groupings
* Add stack support
* Revisit size of uCode ROM
* Look at decoding opcode and addr mode via ROMS and related control unit changes, this would be a new decoder circuit
* Test new ALU and conditional jump operations
  * Add new MC opcodes to support this
* Initialize via via vector
* Add shift/rot operatiosn to ALU and CPU
* Consider design of ALU flag handling, can this be simplified?
* Consider moving ALU registers down into ALU sub-circuit
* Investigate using multiple phases of clocks to reduce execution cycles
* Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions
* Consider direct connection between A and ALUA - this might cut cycles for most ALU operations by 1
* Add flags support for non ALU operations e.g. LDA should set Z and N as appropriate
* Write up current design
  * Architecture
  * Explanation of all control signals
* Have microasm account for where word size does not fit exactly with uROMs
* Improve log output implementation in microasm - its all over the code base at the moment with multiple hard coded numbers
* Consider changing name of ALUP to ALUC now it stores only the carry flag
* Factor out microasm per file section
* Add terminal for output
* Consider adding keyboard for input (likely need interrupt support in CPU first)
