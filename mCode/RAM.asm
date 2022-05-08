            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       LDA #$77
            STA p1
            LDA #$66
            LDX #$11
            LDY #$22
incer:      INX
            BEQ halt
            JMP incer
            
halt:       BRK

; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1

