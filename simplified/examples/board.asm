            PROCESSOR 6502

; Code
            SEG entry
            ORG $0

; Main routine

main:       LDA #$50

            LDX #5
.next:      DEX
            BEQ .done
            JMP .next

.done:      LDA #$50
            ADC #$10
            BRK

