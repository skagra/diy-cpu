            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       LDA #$77
            STA p1
            LDA #$66
incer:      INX
            JMP incer
            
; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1

