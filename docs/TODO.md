# TO DO

* Revisit size of uCode ROM
* Test new ALU and conditional jump operations
  * Add new MC opcodes to support this
* Initialize via via vector
* Add shift/rot operations to ALU and CPU
* Consider design of ALU flag handling, can this be simplified?
* Do we need a latch on the output of the ALU?
* Investigate using multiple phases of clocks to reduce execution cycles
* Consider generating HTML as asm output optionally then we can have mouse overs to ID flags and to link to definitions
* Consider direct connection between A and ALUA - this might cut cycles for most ALU operations by 1
* Add/review setting of P flags for all existing operations
* Write up current design
  * Architecture
  * Explanation of all control signals
* Have microasm account for where word size does not fit exactly with uROMs
* Improve log output implementation in microasm - its all over the code base at the moment with multiple hard coded numbers
* Consider adding keyboard for input (likely need interrupt support in CPU first)
* Check ALU ignores carry in on inc/dec operations
* As we don't hold the result of the ALU in a register op needs to be set on same cycle and moving result. Add a register?
* Check order of pushing bytes of addresses onto stack for JSR (and later BRK)
* Integrate test rig
* Check the substraction logic in the PC ADD circuit
* Take a very careful look at ADC, SBC, CMPs and setting of C and V flags
* Review flag handling throughout
* Review C flag in INC/DEC operations in ALU

