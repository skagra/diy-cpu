# Overview

The project has the following structure:

* `asminclude` - 
* `digital` - 
* `docs` -
* `examples` -
* `microasm` -
* `os` - 
* `tests` -
* `ucode` -

# Tooling

The following tools are needed to build/run `diy-cpu`:

* [Digital](https://github.com/hneemann/Digital) for digital circuit emulation.
* [DASM](https://dasm-assembler.github.io/) to assemble 6502 machine code.
*  [DotNet](https://dotnet.microsoft.com/en-us/download) to build the `diy-cpu`'s microcode assembler (`microsasm`).
* [Visual Studio Code](https://code.visualstudio.com/) is used in the development of the project.  While not strictly necessary using the vscode build targets will significantly oil the wheels!

# Build and Run

1. Run the `all` task in Visual Studio Code.  This will:
   * Compile the microcode assembler (`microasm`).
   * Assemble machine code:

      * `examples` - Example machine code programs to run on the CPU.
      * `os` - The *operating system*.    Actually just an interrupt handler and settings for the `IRQ` vector and reset vector.
      * `tests` - Machine code tests to check all op codes are correctly implemented.
      
1. Run `microasm` to build the µcode ROM images and decoder ROM images used by the CPU:
      * `uROM-[0-5].bin` - µcode routines executed by the *control unit* to orchestrate the operation of the CPU.
      * `mModeDecoder.bin` - Addressing mode decoder ROM.
      * `mOpDecoder.bin` - Machine instruction decoder ROM.
   
   If you run via `Ctrl f5` in Visual Studio Code then the correct parameters will be supplied.  
   
   Otherwise you need to supply the the directory containing the µcode definitions and an output directory.  
   
   For example, from the root directory of the repo:

      `dotnet run --project microasm ucode <output-dir>`

1. Configure the `digital` circuit emulation application to use the built µcode and decoder ROMs.

   * Run `digital` and open `digital\CPU.dig`.
   * Configure the decoder ROMs
      * Right click on the *Instruction Decoder* and select "Open Circuit".
         * Right click on *p1 (Mode)* ROM and select the *Advanced* tab.
         * Set the *File* value to the location of the `uCode/bin/mModeDecoder.bin` built above.
         * Click `OK` to close the dialog.
         * Right click on *p2 (OpCode)* ROM and select the *Advanced* tab.
         * Set the *File* value to the location of the `uCode/bin/mOpDecoder.bin` built above.
         * Click `OK` to close the dialog.
         * Save and close the *Instruction Decoder* window.
      * Right click on the *Control Unit* and select "Open Circuit".
         * One by one right click on each of the `uCode[0-5]` ROM.
         * Select the *Advanced* tab.
         * Set the *File* value to the appropriate `uCode/bin/uROM-[0-5].bin` built above.
         * Click `OK` to close the dialog.
         * When all 6 are done save and close the *Control Unit* window.
      
1. Configure the `digital` circuit emulation application to run chosen machine code.
   * Configure the operating system (just as simple interrupt handler and `IRQ` and reset vectors):
      * Right click on *ROM* and select the *Advanced* tab.
      * Select the *Advanced* tab.
      * Set the *File* value to the the os file `os/bin/os.bin`.
      * Click `OK` to close the dialog.
   * Configure the machine code to run.   The machine code built above from the `examples` directory contains sample machine code programs.  To configure the chosen program:
      * Select from the *Edit* menu *Circuit specific settings*.
      * Select the *Advanced* tab.
      * Set the *File* value to the required machine code e.g. `examples/bin/RAM.bin`.
       * Click `OK` to close the dialog.

1. And finally run the machine code program.
   * Select *Start of Simulation* from the *Simulation* menu.
   * The simulation will start with the CPU halted.  Click the `RUN` button (top left hand corner of the circuit) the start the CPU.

