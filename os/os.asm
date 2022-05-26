    PROCESSOR 6502

; I/O Addresses
TERMINAL        = $4000
POSTL           = $4001
POSTH           = $4002

   ORG   $00, $00

   LDA   #$FF
   STA   POSTH 
   STA   POSTL
   RTI

   ORG $3FFC 

RESET DC.W $0200
IRQ   DC.W $C000
