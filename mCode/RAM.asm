            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       LDA #$AA
            STA p1
            LDX #$EE
incer:      INX
            BEQ halt
            JMP incer
            
halt:       LDX $CC
            LDA $BB
            BRK

; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1

