; from https://codeburst.io/lets-write-some-harder-assembly-language-code-c7860dcceba
; Output: Weekday in A (0=Sunday, 1=Monday, ..., 6=Saturday)

    PROCESSOR 6502

; I/O Addresses
TERMINAL    = $4000
POSTL       = $4001
POSTH       = $4002

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

        LDA     #$16
        STA     MPR
        LDA     #$60
        STA     MPD
        JSR     MULSUB
        LDA     RESULT
        STA     POSTL
        LDA     RESULT+1
        STA     POSTH
LOOP    JMP     LOOP

MULSUB  LDA     #0       ; zero accumulator
        STA     TMP      ; clear address
        STA     RESULT   ; clear
        STA     RESULT+1 ; clear
        LDX     #8       ; x is a counter
MULT    LSR     MPR      ; shift mpr right - pushing a bit into C
        BCC     NOADD    ; test carry bit
        LDA     RESULT   ; load A with low part of result
        CLC
        ADC     MPD      ; add mpd to res
        STA     RESULT   ; save result
        LDA     RESULT+1 ; add rest off shifted mpd
        ADC     TMP
        STA     RESULT+1
NOADD   ASL     MPD      ; shift mpd left, ready for next "loop"
        ROL     TMP      ; save bit from mpd into temp
        DEX              ; decrement counter
        BNE     MULT     ; go again if counter 0
        RTS

RESULT  DC.W
TMP     DC 
MPR     DC
MPD     DC




