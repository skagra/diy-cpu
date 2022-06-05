# Address Decoder

The *Address Decoder* allows one of four memory banks to be enabled based on the incoming address.

![Address Decoder](address-decoder.svg)

| Range             | Bank |
| ----------------- | ---- |
| `0xC000...0xFFFF` | 3    |
| `0x8000...0xBFFF` | 2    |
| `0x4000...0x7FFF` | 1    |
| `0x0000...0x3FFF` | 0    |

The bank is selected based on the 2 most significant bits of the address.

# Inputs

* `A` - Address.

# Outputs

* `CS3` - Address is in the range for bank 3.
* `CS2` - Address is in the range for bank 2.
* `CS1` - Address is in the range for bank 1.
* `CS0` - Address is in the range for bank 0.
  
