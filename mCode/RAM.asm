            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       LDA #$11
            LDX #$CC

inc:        INX
            BEQ halt
            JMP inc
            
halt:       LDA #$DD
            LDY #$FF
            BRK

; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1
