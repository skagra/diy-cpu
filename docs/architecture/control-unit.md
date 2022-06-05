# Control Unit

The control unit governs the operation of the CPU via an array of control lines.   These control lines are driven from *µcode* (microcode) stored in a set of ROMs (`uCode0->uCode5`).  Each µcode instruction are gives the high/low values for each control line.  The act of reading a µcode instruction is effectively its execution.

![Control Unit](control-unit.svg)

Execution of a machine code instruction is divided into the following µcode phases:

* *p0 Fetch* - The opcode at the address given by the `PC` is fetched from memory into the `MBR`.
* *p1 Addressing Mode* - According to the opcode's addressing mode (e.g. immediate), µcode is executed to read any arguments from memory concluding with the resolved address of the actual operand in the `MAR`.
* *p2 OpCode* - According to the opcode (e.g. `LDA`), µcode is executed to carry out the appropriate action (e.g. to load a value into `A`). 

The control unit has the following components:

* `CAR` - The *Control Address Register* (`CAR`) contains the address of the current µcode instruction.
* Sequence logic - The sequence logic calculates the address of the next µcode instruction.  
  It consists of a multiplexer which selects from:
  * `CAR` + 1 - The address of the subsequent µcode instruction.
  * `p0 (Fetch)` Register - Contains the address of `p0` µcode.
  * `MODEADDR` - The address of the µcode routine associated with the addressing mode of current machine opcode.
  * `OPCDOEADDR` - The address of the µcode routine associated with the current machine opcode.
  * `UPARAM` - A sixteen bit operand that forms part of each µcode instruction.  Typically it is used to hold the target address µcode jumps (`uJMP`, `uNJMP`, `uVJMP`, `uZJMP`, `uCJMP`).
  * `IRQ uCode` Register - Contains the address of µcode triggered via an `IRQ`.
  Selection logic to to pick from the above according to the values of the following bits in the current µcode instruction `IRQ`, `uP0`, `uP1`, `uP2`, `uJMP`, `uNJMP`, `uVJMP`, `uZJMP`, `uCJMP`, `uJMPINV`, `N`, `V`, `I`, `Z`  and `C`.  These are described below.
* `p0 (Fetch)` Register - Contains the address of `p0` µcode.   Its value is populated via µcode during initialization.
* `IRQ uCode` Register - Contains the address of µcode to run when an interrupt is signalled.  Its value is populated via µcode during initialization.
* µcode ROMs `uCode0` -> `uCode1`.  The word defining each µcode instruction is divided across the µcode ROMs.  The address of the current µcode instruction is presented to these ROMs to read a µcode instruction.  The act of reading a µcode sets the control lines it defines.  See below for further details.
* `CAR` Adder - Used to calculate the address of the subsequent µcode instruction (`CAR`+1).
* Error detection - Unknown addressing modes and machine opcodes decode to `0xFFFF` which causes the control unit to flag an error.

## Microinstruction Format

µcode instructions are 80 bits in length with the following format:

```
   0xSSSSSSSSAA
```

`S` bytes are control signals.  `A` bytes are a parameter for a µcode operation, most commonly the µcode target address for `uJMP`, `uZJMP`, `uNJMP`, `uVJMP`, `uCJMP` and `uJSR`.

µcode instructions are stored in little-endian order when written to the ROM files by the microcode assembler `microasm`.

```
   Natural order                    uROM (little Endian) order
   S9 S8 S7 S6 S5 S4 S3 S2 A1 A0 => AO A1 S2 S3 S4 S5 S6 S7 S8 S9 
```

Each `S` bit corresponds either to a control line to set or to an internal control condition.  A complete µcode instruction is the `or` of these bits together with option the `A` parameter bytes.

## Microinstructions & Control Lines

This section describes all implemented the µcode instructions. 

### Control Unit Operations

| Instruction | Meaning                                                                                          |
| ----------- | ------------------------------------------------------------------------------------------------ |
| `uP0INIT`   | Initialize the `p0 Fetch` register from the `A` parameter bytes of the µcode instruction.        |
| `uIRQINIT`  | Initialize the `IRQ uCode` register from the `A` parameter bytes of the µcode instruction.       |
| `uP0`       | Jump to `p0` µcode.                                                                              |
| `uP1`       | Jump to `p1` µcode corresponding to the addressing mode of the current machine code instruction. |
| `uP2`       | Jump to `p2` µcode corresponding to the the current machine code instruction.                    |
| `uJMP`      | Jump to the µcode at `A`.                                                                        |
| `uNJMP`     | Jump to the µcode at `A` if `N` is set.                                                          |
| `uVJMP`     | Jump to the µcode at `A` if `V` is set.                                                          |
| `uZJMP`     | Jump to the µcode at `A` if `Z` is set.                                                          |
| `uCJMP`     | Jump to the µcode at `A` if `C` is set.                                                          |
| `uJMP/INV`  | Invert the logic a`u*JMP`. instructions.                                                         |

### Constant Values

These constant values used during initialize/reset.

| Instruction   | Meaning                        |
| ------------- | ------------------------------ |
| `CDATA/LD/0`  | Set the `CDATA` bus to `0x00`. |
| `CDATA/LD/FF` | Set the `CDATA` bus to `0xFF`. |

### Paths Between Busses

| Instruction       | Meaning                                               |
| ----------------- | ----------------------------------------------------- |
| `CDATA/TO/CADDRL` | Open a path from the `CDATA` bus to the `CADDRL` bus. |
| `CDATA/TO/CADDRH` | Open a path from the `CDATA` bus to the `CADDRH` bus. |

### External Data Bus/Memory

| Instruction     | Meaning                                                                   |
| --------------- | ------------------------------------------------------------------------- |
| `MEM/LD/XDATA`  | Load memory on the `XDATA` bus into memory at the address given by `MAR`. |
| `MEM/OUT/XDATA` | Write the value of memory at address given by `MAR` onto the `XDATA` bus. |
| `IR/LD/XDATA`   | Load `IR` from the `XDATA` bus.                                           |
| `MBR/LD/XDATA`  | Load `MBR` from the `XDATA` bus.                                          |
| `MBR/OUT/XDATA` | Write `MBR` onto the `XDATA` bus.                                         |

### MBR and the Internal Data Bus

| Instruction     | Meaning                          |
| --------------- | -------------------------------- |
| `MBR/LD/CDATA`  | Load `MBR` from the `CDATA` bus. |
| `MBR/OUT/CDATA` | Wite `MBR` onto the `CDATA` bus. |

### MARL and the Internal Address Bus

| Instruction      | Meaning                            |
| ---------------- | ---------------------------------- |
| `MARL/LD/CADDRL` | Load `MARL` from the `CADDRL` bus. |
| `MARH/LD/CADDRH` | Load `MARH` from the `CADDRH` bus. |

### Program Counter

| Instruction     | Meaning                                          |
| --------------- | ------------------------------------------------ |
| `PC/INC`        | Increment `PC`                                   |
| `PC/REL/CDATA`  | Add the signed value on the `CDATA` bus to `PC`. |
| `PCL/LD/CDATA`  | Load `PCL` from the `CDATA` bus.                 |
| `PCL/OUT/CDATA` | Write `PCL` to the `CDATA` bus.                  |
| `PCH/LD/CDATA`  | Load `PCH` from the `CDATA` bus.                 |
| `PCH/OUT/CDATA` | Write `PCH` to the `CDATA` bus.                  |
| `PC/OUT/CADDR`  | Write `PC` to the `CADDR` bus.                   |

### General Purpose Registers

| Instruction        | Meaning                             |
| ------------------ | ----------------------------------- |
| `STASHL/LD/CDATA`  | Load `STASHL` from the `CDATA` bus. |
| `STASHL/OUT/CDATA` | Write `STASHL` to the `CDATA` bus.  |
| `STASHH/LD/CDATA`  | Load `STASHH` from the `CDATA` bus. |
| `STASHH/OUT/CDATA` | Write `STASHH` to the `CDATA` bus.  |
| `A/LD/CDATA`       | Load `A` from the `CDATA` bus.      |
| `A/OUT/CDATA`      | Write `A` to the `CDATA` bus.       |
| `X/LD/CDATA`       | Load `X` from the `CDATA` bus.      |
| `X/OUT/CDATA`      | Write `X` to the `CDATA` bus.       |
| `Y/LD/CDATA`       | Load `Y` from the `CDATA` bus.      |
| `Y/OUT/CDATA`      | Write `Y` to the `CDATA` bus.       |

### Status (P) Flags

| Instruction | Meaning            |
| ----------- | ------------------ |
| `PN/LD`     | Load the `N` flag. |
| `PV/LD`     | Load the `V` flag. |
| `PV/LD`     | Load the `B` flag. |
| `PI/LD`     | Load the `I` flag. |
| `PZ/LD`     | Load the `Z` flag. |
| `PC/LD`     | Load the `C` flag. |

The source of the value loaded into `P` flags is controlled by:

```
P/SRC/0          
P/SRC/1                
P/SRC/2   
```

As follows:

| P/SRC/* | Source                        |
| ------- | ----------------------------- |
| `000`   | Reset.                        |
| `001`   | Set.                          |
| `010`   | `ALU` out.                    |
| `011`   | Flags processed from `CDATA`. |
| `100`   | Raw `CDATA`.                  |
| `1xx`   | N/A.                          |
       

| Instruction   | Meaning                       |
| ------------- | ----------------------------- |
| `P/OUT/CDATA` | Write `P` to the `CDATA` bus. |

### ALU

| Instruction      | Meaning                                  |
| ---------------- | ---------------------------------------- |
| `ALUA/LD/CDATA`  | Load `ALUA` from the `CDATA` bus.        |
| `ALUB/LD/CDATA`  | Load `ALUB` from the `CDATA` bus.        |
| `ALUC/LD`        | Load `ALUC` from the `CDATA` bus.        |
| `ALUR/OUT/CDATA` | Write the ALU result to the `CDATA` bus. |

The source of the value loaded into `ALUC` is controlled by:

```
ALUC/SRC/0       
ALUC/SRC/1       
```

As follows:

| ALUC/SRC/* | Meaning         |
| ---------- | --------------- |
| `00`       | Reset.          |
| `01`       | Set.            |
| `10`       | `P` flag (`C`). |
| `11`       | N/A.            |

The operation carried out by the `ALU` is controlled by:

```
ALUOP/0         
ALUOP/1          
ALUOP/2         
ALUOP/3  
```

As follows:

| ALU/OP/* | Meaning |
| -------- | ------- |
| `0000`   | AND     |
| `0001`   | OR      |
| `0010`   | NOT     |
| `0011`   | XOR     |
| `0100`   | ADD     |
| `0101`   | SUB     |
| `0110`   | INC     |
| `0111`   | DEC     |
| `1000`   | SHIFT-R |
| `1001`   | SHIFT-L |
| `1010`   | ROT-R   |
| `1011`   | ROT-L   |
        
### Stack Pointer

| Instruction   | Meaning                                                    |
| ------------- | ---------------------------------------------------------- |
| `S/INC`       | Increment `S`.                                             |
| `S/DEC`       | Decrement `S`.                                             |
| `S/LD/CDATA`  | Load `S` from the `CDATA` bus.                             |
| `S/OUT/CDATA` | Write `S` to the `CDATA` bus.                              |
| `S/OUT/CADDR` | Write `S` to the `CADDR` bus (high byte is always `0x01`). |

### Control Unit

| Instruction   | Meaning                                   |
| ------------- | ----------------------------------------- |
| `CUP/SRC/ALU` | Source of `P` flags for the control unit. |

Where:

| Value | Meaning                           |
| ----- | --------------------------------- |
| `0`   | Read flags from `P`.              |
| `1`   | Read flags from the `ALU` output. |

### State

| Instruction | Meaning                                                                 |
| ----------- | ----------------------------------------------------------------------- |
| `CPU/RESET` | Reset to the CPU. `PC` will be set to the reset vector.                 |
| `CPU/HALT`  | Halt the CPU                                                            |
| `CPU/IRQ`   | Flag a `IRQ/BRK` to the CPU - `PC` will be set to the `IRQ/BRK` vector. |

## Inputs

| Name         | Function                                                                                          |
| ------------ | ------------------------------------------------------------------------------------------------- |
| `CLOCK`      | System clock.                                                                                     |
| `N`          | `N` flag.                                                                                         |
| `V`          | `V` flag.                                                                                         |
| `I`          | `I` flag.                                                                                         |
| `Z`          | `Z` flag.                                                                                         |
| `C`          | `C` flag.                                                                                         |
| `MODEADDR`   | Address of the µcode that implements the addressing mode of the current machine code instruction. |
| `OPCODEADDR` | Address of the µcode that implements the current machine code instruction.                        |
| `RESET`      | A request to reset the CPU.                                                                       |
| `IRQ`        | An interrupt request.                                                                             |

## Outputs

In addition to an output for each control line bit corresponding to a µcode instruction, the control unit has the following outputs.

| Name           | Function                                                                                    |
| -------------- | ------------------------------------------------------------------------------------------- |
| `CPU/ERR`      | A error condition has been detected.                                                        |
| `<CAR>`        | Debugging output giving the value of `CAR`                                                  |
| `<UPARAM>`     | Debugging output giving the value the `A` bytes parameter of the current µcode instruction. |
| `<CARNEXT>`    | Debugging output giving the address of the next µcode instruction.                          |
| `<CARNEXTSRC>` | Debugging output giving the source of the address of the next µcode instruction.            |
  
