# DIY CPU

This project is a learning exercise in digital electronics, with goals to:

* Design a CPU from the ground up that is op-code compatible with the 6502.
* Implement and validate the design in a digital simulator.
* Implement the design physically, primarily using the `74` family of ICs.
* 
# Status

The CPU is approaching feature completeness in the digital simulator.  

The following is still to do:

* BCD mode and D flag
* B Flag 

# Building

The following tools are needed to build/run DIY CPU:

* Digital Circuit Emulation: [Digital](https://github.com/hneemann/Digital)
* 6502 Assembler: [DASM](https://dasm-assembler.github.io/)
* Microasm: [DotNet](https://dotnet.microsoft.com/en-us/download)

While not required, the [Visual Studio Code](https://code.visualstudio.com/) is used for the project so all build targets etc. should work out of the box.