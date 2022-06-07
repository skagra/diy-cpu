# DIY CPU

This project is a learning exercise in digital electronics, with goals to:

* Design a CPU from the ground up that is op-code compatible with the 6502.
* Implement and validate the design in a digital circuit simulator.
* Implement the design physically, primarily using the `74` family of ICs.

![Overview](docs/architecture/overview.png)

* The design is documented [here](docs/architecture/index.md).
* Build/execution instructions are available [here](docs/build-and-run.md).

# Status

The CPU is approaching feature completeness in the digital circuit simulator.  

The following is still to do:

* BCD mode and `D` flag
* `B` Flag 