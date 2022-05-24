
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

        LDA #$4
        JSR MULT10
        STA POST
        BRK
        
MULT10  ASL         ;multiply by 2
        STA TEMP    ;temp store in TEMP
        ASL         ;again multiply by 2 (*4)
        ASL         ;again multiply by 2 (*8)
        CLC
        ADC TEMP    ;as result, A = x*8 + x*2
        RTS

TEMP    .byte 0