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
| Zero page, Y | `STA $00,Y`   | The zero page argument is added to Y to give the zero page address of the operand.  Addition wraps to zero page.   