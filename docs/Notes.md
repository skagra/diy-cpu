# SBC and CMP

* http://www.6502.org/tutorials/compare_beyond.html
* https://www.righto.com/2013/01/a-small-part-of-6502-chip-explained.html
* http://www.6502.org/tutorials/vflag.html
* https://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
* http://6502.cdot.systems/
* https://www.masswerk.at/6502/

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



  