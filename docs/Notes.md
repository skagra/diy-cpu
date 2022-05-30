# Control Unit

The control unit governs the operation of the CPU via an array of control lines.   These control lines are driven from µcode (microcode) contained in a set of ROMs (`uCode0->uCode5`).  µcode instructions are decoded by these ROMs to give high/low values for each control line.  The act of reading a µcode is effectively its execution.

![Control Unit](Control%20Unit.svg)

Execution of a machine code instruction is divided into the following µcode phases:

* *p0 Fetch* - The opcode at address given by the `PC` is fetched from memory into the `MBR`.
* *p1 Addressing Mode* - According to opcode's addressing mode (e.g. immediate), µcode is executed to read any arguments memory and to populated `MAR` with the resolved address of the actual operand.
* *p2 OpCode* - According on the opcode (e.g. `LDA`), µcode is executed to carry out the appropriate action (e.g. to load a value into `A`). 

The control unit has the following components:

* `CAR` - The *Control Address Register* (`CAR`) contains the address of the current µcode instruction.
* Sequence logic - The sequence logic calculates the address of the next µcode instruction.  
  It consists of a multiplexer which selects from:
  * `CAR` + 1 - The address of the next µcode instruction.
  * `p0 (Fetch)` Register - Contains the address of `p0` µcode.
  * `MODEADDR` - The address of the µcode routine associated with the address mode of current machine opcode.
  * `OPCDOEADDR` - The address of the µcode routine associated with the current machine opcode.
  * `UPARAM` - A sixteen bit operand that forms part of each µcode instruction.  Typically it is used to hold the target address µcode jumps (`uJMP`, `uNJMP`, `uVJMP`, `uZJMP`, `uCJMP`).
  * `IRQ uCode` Register - Contains the address of µcode trigger via an `IRQ`.
  Selection logic to to pick from the above according to the values of the following bits in the current µcode instruction `IRQ`, `uP0`, `uP1`, `uP2`, `uJMP`, `uNJMP`, `uVJMP`, `uZJMP`, `uCJMP`, `uJMPINV`, `N`, `V`, `I`, `Z`  and `C`.  These are described in below.
* `p0 (Fetch)` Register - Contains the address of `p0` µcode.   It's value is populated via µcode during initialization.
* `IRQ uCode` Register - Contains the address of µcode to run when an interrupt is signalled.  It's value is populated via µcode during initialization.
* µcode ROMs `uCode0` -> `uCode1`.  The word defining each µcode instruction is divided across the µcode ROMs.  The address of the current µcode instruction is divided and presented to these ROMs to read µcode instruction effectively setting the required control lines.  See below for further details.
* `CAR` Adder - Used to calculate the address of the subsequent µcode instruction (`CAR`+1).
* Error detection - Unknown addressing mode and machine opcodes decode to `0xFFFF` which causes the control unit to flag an error.

## Microinstruction Format

µcode instructions are 80 bits in length with the following format:

   ```
   0xSSSSSSSSAA
   ```

Where `S` bytes are control signals, and `A` bytes are a parameter for a µcode operation, most commonly the µcode target address for `uJMP`, `uZJMP`, `uNJMP`, `uVJMP`, `uCJMP` and `uJSR`.

Microinstructions are stored little-endian order when written to the output ROM file by microcode assembler `microasm`.

```
Natural order                    uROM (little endian) order
S9 S8 S7 S6 S5 S4 S3 S2 A1 A0 => AO A1 S2 S3 S4 S5 S6 S7 S8 S9 
```

Each `S` bit corresponds either to a control line to set or to an internal control condition.  A complete µcode instruction is the `or` of these bits together with option the `A` parameter bytes.

## Microinstructions & Control Lines

The following µcode are supported:

### Control Unit Operations

| Instruction | Meaning |
|-------------|---------|
| `uP0INIT`   | Initialize the `p0 Fetch` register from the `A` parameter bytes of the instruction.            
| `uIRQINIT`  | Initialize the `IRQ uCode` register from the `A` parameter bytes of the instruction. 
| `uP0`       | Jump to `p0` microcode.        
| `uP1`       | Jump to `p1` microcode corresponding to the addressing mode of the current machine code instruction.    
| `uP2`       | Jump to `p2` microcode corresponding to the the current machine code instruction.     
| `uJMP`      | Jump to the µcode at `A`.      
| `uNJMP`     | Jump to the µcode at `A` if `N` is set.          
| `uVJMP`     | Jump to the µcode at `A` if `V` is set.         
| `uZJMP`     | Jump to the µcode at `A` if `Z` is set.         
| `uCJMP`     | Jump to the µcode at `A` if `C` is set.     
| `uJMP/INV`  | Inverts the logical of all `u*JMP` instructions.    

### Constant Values

These constant values used during initialize/reset.

| Instruction   | Meaning |
|---------------|---------|
| `CDATA/LD/0`  | Set `CDATA` bus to `0x00`.    
| `CDATA/LD/FF` | Set `CDATA` bus to `0xFF`.

### Paths Between Busses

| Instruction       | Meaning |
|-------------------|---------|
| `CDATA/TO/CADDRL` | Open path from `CDATA` to `CADDRL`. 
| `CDATA/TO/CADDRH` | Open path from `CDATA` to `CADDRH`.

### External Data Bus/Memory

| Instruction       | Meaning |
|-------------------|---------|
| `MEM/LD/XDATA`    | Load memory at address `MAR` with value on `XDATA` bus.    
| `MEM/OUT/XDATA`   | Write value of memory at `MAR` onto the `XDATA` bus.
| `IR/LD/XDATA`     | Load `IR` from the `XDATA` bus.  
| `MBR/LD/XDATA`    | Load `MBR` from the `XDATA` bus.
| `MBR/OUT/XDATA`   | Write `MBR` onto the `XDATA` bus.

### MBR and the Internal Data Bus

| Instruction       | Meaning |
|-------------------|---------|
| `MBR/LD/CDATA`    | Load `MBR` from the `CDATA` bus.   
| `MBR/OUT/CDATA`   | Wite `MBR` onto the `CDATA` bus.

### MARL and the Internal Address Bus

| Instruction       | Meaning |
|-------------------|---------|
| `MARL/LD/CADDRL`  | Load `MARL` from the `CADDRL` bus. 
| `MARH/LD/CADDRH`  | Load `MARH` from the `CADDRH` bus.

### Program Counter

| Instruction       | Meaning |
|-------------------|---------|
| `PC/INC`          | Increment `PC` |   
| `PC/REL/CDATA`    | Add signed value on the `CDATA` bus to `PC`.
| `PCL/LD/CDATA`    | Load `PCL` from the `CDATA` bus.
| `PCL/OUT/CDATA`   | Write `PCL` to the `CDATA` bus.
| `PCH/LD/CDATA`    | Load `PCH` from the `CDATA` bus.
| `PCH/OUT/CDATA`   | Write `PCH` to the `CDATA` bus.
| `PC/OUT/CADDR`    | Write `PC` to the `CADDR` bus.

### General Purpose Registers

| Instruction        | Meaning |
|--------------------|---------|
| `STASHL/LD/CDATA`  | Load `STASHL` from the `CDATA` bus.
| `STASHL/OUT/CDATA` | Write `STASHL`` to the `CDATA` bus. 
| `STASHH/LD/CDATA`  | Load `STASHH` from the `CDATA` bus. 
| `STASHH/OUT/CDATA` | Write `STASHH` to the `CDATA` bus.
| `A/LD/CDATA`       | Load `A` from the `CDATA` bus.       
| `A/OUT/CDATA`      | Write `A` to the `CDATA` bus.     
| `X/LD/CDATA`       | Load `X` from the `CDATA` bus.      
| `X/OUT/CDATA`      | Write `X` to the `CDATA` bus.    
| `Y/LD/CDATA`       | Load `Y` from the `CDATA` bus.      
| `Y/OUT/CDATA`      | Write `Y` to the `CDATA` bus.     

## Status (P) Flags

| Instruction        | Meaning |
|--------------------|---------|
| `PN/LD`            | Load the `N` flag.           
| `PV/LD`            | Load the `V` flag.            
| `PI/LD`            | Load the `I` flag.            
| `PZ/LD`            | Load the `Z` flag.           
| `PC/LD`            | Load the `C` flag.            

The source of the value loaded to `P` flags is controlled by:

   ```
   P/SRC/0          
   P/SRC/1                
   P/SRC/2   
   ```

As follows:

| P/SRC | Meaning |
|-------|---------|
| `000` | Reset. 
| `001` | Set.
| `010` | `ALU` out.
| `011` | Flags processed from `CDATA`.
| `100` | Raw `CDATA`.
| `1xx` | N/A.
       

| Instruction        | Meaning |
|--------------------|---------|
| `P/OUT/CDATA` | Write `P` to the `CDATA` bus.      

### ALU

| Instruction        | Meaning |
|--------------------|---------|
| `ALUA/LD/CDATA`    | Load `ALUA` from the `CDATA` bus.  
| `ALUB/LD/CDATA`    | Load `ALUB` from the `CDATA` bus.
| `ALUC/LD`          | Load `ALUC` from the `CDATA` bus.
| `ALUR/OUT/CDATA`   | Write the ALU result to the `CDATA` bus.

The source of value loaded into `ALUC` is controlled by:

   ```
   ALUC/SRC/0       
   ALUC/SRC/1       
   ```
As follows:

| ALUC/SRC | Meaning |
|----------|---------|
| `00` | Reset.
| `01` | Set.
| `10` | `P` flag (`C`).
| `11` | N/A.

The operation carried out by the `ALU` is controlled by:

   ```
   ALUOP/0         
   ALUOP/1          
   ALUOP/2         
   ALUOP/3  
   ```
As follows:

| ALU/OP | Meaning |
|--------|---------|
| `0000` | AND
| `0001` | OR
| `0010` | NOT
| `0011` | XOR
| `0100` | ADD
| `0101` | SUB
| `0110` | INC
| `0111` | DEC
| `1000` | SHIFT-R
| `1001` | SHIFT-L
| `1010` | ROT-R
| `1011` | ROT-L
        
### Stack Pointer

| Instruction   | Meaning |
|---------------|---------|
| `S/INC`       | Increment `S`.     
| `S/DEC`       | Decrement `S`.
| `S/LD/CDATA`  | Load `S` from the `CDATA` bus.
| `S/OUT/CDATA` | Write `S` to the `CDATA` bus.
| `S/OUT/CADDR` | Write `S` to `CADDR` (high byte is always `0x01`).

### Control Unit

| Instruction        | Meaning |
|--------------------|---------|
| `CUP/SRC/ALU` | Source of `P` flags for the control unit.|

Where:

| Value | Meaning |
|-------|---------|
| `0` | Read flags from `P` register.
| `1` | Read flags from `ALU` output. 

### State

| Instruction | Meaning |
|-------------|---------|
| `CPU/RESET` | Reset to the CPU - PC will be set to the reset vector     
| `CPU/HALT`  | Halt the CPU           
| `CPU/IRQ`   | Flag a `IRQ/BRK` to the CPU - PC will be set to the `IRQ/BRK` vector        

# Memory Map

```
  +===========+ 
  |           | FEEE
  |    ROM    |
  |           | C000
  +===========+
  |           | BFFF
  |    N/A    | 
  |           | 8000 
  +===========+
  |           | 7FFF
  |    I/O    | 
  |           | 4000
  +===========+
  |           | 3FFF
  |    RAM    |
  |           | 0200
  |...........| 
  |           | 01FF
  |   Stack   | 
  |           | 0100 
  +...........+
  |           | 00FF
  | Zero Page |
  |           | 0000
  +===========+     
```

# Addressing Modes

| Mode         | Example       | Description                                                                                                                                                                                                                                                                                                                    |
| ------------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Accumulator  | `ASL A`       | Implicitly targets the accumulator                                                                                                                                                                                                                                                                                             |
| Absolute     | `ADC $4400`   | Argument gives the address of the operand.                                                                                                                                                                                                                                                                                     |
| Absolute, X  | `ADC $4400,X` | Argument is added to X (with carry) to give the two byte address of the operand.                                                                                                                                                                                                                                               |
| Absolute, Y  | `ADC $4400,Y` | Argument is added to Y (with carry) to give the two byte address of the operand.                                                                                                                                                                                                                                               |
| Immediate    | `ADC #$44`    | Argument is the operand.                                                                                                                                                                                                                                                                                                       |
| Implied      | `TSX`         | No operand                                                                                                                                                                                                                                                                                                                     |
| Indirect     | `JMP ($5597)` | Argument gives the address of a two byte value used as the target of a JMP operation. **Note** that in a 6502 the calculation of the address of the second indirected byte is done without carry!  So the operation will not give the expected result where the argument refers to the last byte in a page. Used only for JMP. |
| X, Indirect  | `ADC ($44,X)` | The one byte argument is added to X to give a zero-page address.  The two byte value at that address points to the operand.  Addition wraps to zero-page.                                                                                                                                                                      |
| Indirect, Y  | `ADC ($44),Y` | The argument points to a two byte zero page value which is retrieved and added to Y to give address of the operand.                                                                                                                                                                                                            |
| Relative     | `BPL $50`     | The operand is an 8 bit signed offset from the PC.                                                                                                                                                                                                                                                                             |
| Zero page    | `ADC $44`     | The argument is the zero-page address of the operand                                                                                                                                                                                                                                                                           |
| Zero page, X | `ADC $44,X`   | The zero page argument is added to X to give the zero page address of the operand.  Addition wraps to zero page.                                                                                                                                                                                                               |
| Zero page, Y | `STA $00,Y`   | The zero page argument is added to Y to give the zero page address of the operand.  Addition wraps to zero page.                                                                                                                                                                                                               |

# SBC and CMP                                     |

http://www.6502.org/tutorials/compare_beyond.html
https://www.righto.com/2013/01/a-small-part-of-6502-chip-explained.html
http://www.6502.org/tutorials/vflag.html
https://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
http://6502.cdot.systems/
https://www.masswerk.at/6502/

Noting SBC actually does 1's compliment subtraction - so need to set the carry flag!

`CMP` is equivalent of:

```
SEC
SBC NUM
```

Only effecting `NZC`

`SBC` uses 1 complement!  So to get a proper subtraction

```
SEC
SBC
```

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
* Synertek 6502 programming manual - http://archive.6502.org/datasheets/synertek_programming_manual.pdf
* Test rig - www.baltissen.org/zip/rb65-11.zip
  