
        PROCESSOR 6502

        INCDIR "../asminclude"
        INCLUDE "io.h"
        
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
        STA POSTL
LOOP    JMP LOOP
        
MULT10  ASL         ;multiply by 2
        STA TEMP    ;temp store in TEMP
        ASL         ;again multiply by 2 (*4)
        ASL         ;again multiply by 2 (*8)
        CLC
        ADC TEMP    ;as result, A = x*8 + x*2
        RTS

TEMP    .byte 0