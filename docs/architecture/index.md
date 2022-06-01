# Overview

The following diagram shows a high level block view of the CPU together with some external components.

![Overview](overview.png)

* RAM/ROM -
* Address Decoding -
* `XADDR` and `XDATA` Buses - The *eXternal ADDRess bus* (`XADDR`) and the *eXternal DATA bus* (`XDATA`) connect the CPU with memory (RAM/ROM) and memory mapped I/O.
* `MARH/MARL` and `MBR` Registers - The *Memory ADDRess High/Low* registers (`MARH/MARL`) target a memory location and the *Memory Buffer Register* (`MBR`) stores data read from/to be written to, memory.
* `IR` Instruction Register - The *Instruction Register* (`IR`) stores the currently executing machine code instruction read from memory.
* `CADDRH/CADDRL` Registers - The *Cpu ADDRess High/Low* buses (`CADDRH/CADDRL`) allow memory addresses calculated within the CPU to be transferred to `MARH/MARL`.                                 
* `PCH/PCL` Registers - The *Program Counter High/Low* registers (`PCH/PCL`) hold the memory address of the currently executing machine code instruction. 
* `CDATA` Bus - The *Cpu Data bus* (`CDATA`) allows data transfer within the CPU.  
* `S` Stack Register - The *Stack register* (`S`) points to the next free stack location in memory. 
* `P` Status Register - The *status register* (`P`) holds CPU status flags, for example the `C` flag which signals carry from a proceeding `ALU` addition. 
* `STASHH`/`STASHL` Internal Registers -
* `A`/`X`/`Y` General Purpose Registers - 
* `ALU` Arithmetic Logic Unit - 
* Instruction Decoder -
* Control Unit - 

The design is described in detail in the following:

* [CPU](cpu.md)
* [ALU](alu.md)
* [Control Unit](control-unit.md)
* [Instruction Decoder](instruction-decoder.md)
* [Program Counter](pc.md)
* [Register](register.md)

