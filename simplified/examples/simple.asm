            PROCESSOR 6502

; Code
            SEG entry
            ORG $0

; Main routine
main:       LDA #$50
            TAX
            LDA #00

sayhello:   LDX #0
.next:      LDA hello,X
            BEQ .done
            STA $F2
            INX 
            JMP .next
.done:      

hello:      DC    "Hello World" 
            DC.B  0
