            PROCESSOR 6502

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       ; JSR sayhello
            LDA #$48
            STA $4005
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

sayhello:   LDX 0
next:       LDA hello,X
            BEQ hellodone
            STA $4005
            INX 
            JMP next
hellodone:  RTS

hello:      DC    "Hello World" 
            DC.B  0

; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1
