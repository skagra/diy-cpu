# TO DO

* Revisit size of uCode ROM
* ALU
  * Consider design of ALU flag handling, can this be simplified?
  * Do we need a latch on the output of the ALU?
  * Consider direct connection between A and ALUA - this might cut cycles for most ALU operations by 1
  * Check the subtraction logic in the PC ADD circuit
  * Take a very careful look at ADC, SBC, CMPs and setting of C and V flags
* Clock
  * Investigate using multiple phases of clocks to reduce execution cycles
* Microasm
  * Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions
  * Have microasm account for where word size does not fit exactly with uROMs
  * Complete validation routines
  * Improve log output implementation in microasm - its all over the code base at the moment with multiple hard coded numbers
* Write up current design
  * Architecture
  * Explanation of all control signals
  * etc.
* Microcode  
  * Check order of pushing bytes of addresses onto stack for JSR (and later BRK)
  * BRK is this a one byte or two byte opcode?
  * Add/review setting of P flags for all existing operations
* Finish integrating test rig
* IRQ implementation still needs work
* B flag
* BDC implement (and D flag)


