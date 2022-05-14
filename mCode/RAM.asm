            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       LDA #$23
            STA $4000
            JSR l1 
            LDA #$EE
            PHA
            LDA #$22
            PLA
            TAX

inc:        INX
            BEQ halt
            JMP inc
            
halt:       LDA #$DD
            LDY #$FF
            BRK

l1:         LDA #01
            JSR l2
            LDA #03
            RTS

l2:         LDA #02
            RTS
; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1
