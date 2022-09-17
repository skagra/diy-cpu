
# Status

# Simplified Version

## September 2022

* I've created a simplified version of the design - adhering to the same design principles as the full version but scaling back to make building the circuit a more practical proposition.  This is design is in the [simplified](../simplified/) directory.   This design has been converted into `74HCxx` logic.

* I've built an [EEPROM programmer](https://github.com/skagra/eeprom-programmer) to flash the decoder and EEPROMs.

* Some physical building has begun - but is likely a long haul!

# Full Version

The CPU is approaching feature completeness in the digital circuit simulator.  It is able to execute most 6502 machine code programs.

The following are still to do:

* BCD mode and `D` flag.
* `B` Flag 

Work has started converting the generic design to 74xx ICs.  So far the ALU and Register have been completed.
