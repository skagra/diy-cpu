            PROCESSOR 6502

; Code
            SEG entry
            ORG $00

; Main routine
main:       

; Register loading
            LDA #$50
            LDX #$22
            LDA #$00
            LDX #$00

; Register transfer
            LDA #$67
            TAX
            LDX #$56
            TXA

; Arithmetic
            LDA #$27
            ADC #$23 ; Result $4A
            LDA #$69
            SBC #$33 ; Result $36

; Looping BEQ
            LDX #$05
.nextbeq:   DEX
            BEQ .beqdone
            JMP .nextbeq
.beqdone

; Looping BNE
            LDX #$05
.nextbne:   DEX
            BNE .nextbne

; Indexed addressing
            LDX #$00
.nextfib:   LDA .fib,X
            BEQ .fibdone
            INX 
            JMP .nextfib
.fibdone:   DC.B    0

.fib:       DC.B    1, 1, 2, 3, 5, 8, 0  

