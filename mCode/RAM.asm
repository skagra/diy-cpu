            PROCESSOR 6502

; I/O Addresses
TERMINAL = $4000
POST = $4001

; Skip zero page
            SEG skip-zero-page
            ORG $0
            DS $100 

; Skip stack
            SEG skip-stack
            ORG $100
            DS $100

; Code
            SEG entry
            ORG $200

; Main routine
main:       LDA #$50
            CMP #$60
            CMP #$40
            CMP #$50
            LDA #$22
            STA POST
            ASL      
            JSR sayhello
            JSR l1 
            LDA #$EE
            PHA
            LDA #$22
            PLA
            TAX
            LDX #$F0
.inc:       INX
            BEQ .decs
            JMP .inc
.decs:      LDX #10
.dec        DEX
            BNE .dec

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

            SEG variables
            ORG $6FD
cval:       DC  2

hello:      DC    "Hello World" 
            DC.B  0

; Zero page 
            SEG.U zero-page
            ORG $0


