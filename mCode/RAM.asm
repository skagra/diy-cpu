            PROCESSOR 6502

; I/O Addresses
TERMINAL = $4000

; Entry point - always start of first segment
            SEG entry
            ORG $0

; Main routine
main:       JSR sayhello
            JSR l1 
            LDA #$EE
            PHA
            LDA #$22
            PLA
            TAX
.inc:       INX
            BEQ halt
            JMP .inc
            
halt:       LDA #$DD
            LDY #$FF
            BRK

l1:         LDA #01
            JSR l2
            LDA #03
            RTS

l2:         LDA #02
            RTS

            SUBROUTINE
sayhello:   LDX #0
.next:      LDA hello,X
            BEQ .done
            STA TERMINAL
            INX 
            JMP .next
.done:      RTS

            ORG $4FD
hello:      DC    "Hello World" 
            DC.B  0

; Zero page variables
            SEG.U variables
            ORG $100

p1:         DS 1
