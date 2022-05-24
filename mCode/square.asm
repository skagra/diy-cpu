; Calculates the 16 bit unsigned integer square of the signed 16 bit integer in
; Numberl/Numberh.  The result is always in the range 0 to 65025 and is held in
; Squarel/Squareh
;
; The maximum input range is only +/-255 and no checking is done to ensure that
; this is so.
;
; This routine is useful if you are trying to draw circles as for any circle
;
; x^2+y^2=r^2 where x and y are the co-ordinates of any point on the circle and
; r is the circle radius
;
; Destroys all registers

            PROCESSOR 6502

; I/O Addresses
POST = $4001

; Skip zero page
            SEG zero-page
            ORG $0
            DS $100 

; Skip stack
            SEG stack
            ORG $100
            DS $100

; Code
            SEG entry
            ORG $200

            LDA   #$6
            STA   Numberl
            LDA   #$0
            STA   Numberh
            JSR   Square
            LDA   Squarel
            STA   POST
            BRK

Square:  
            LDA   #$00        
            STA   Squarel     
                             
            LDA   Numberl     ; get number low byte
            LDX   Numberh     ; get number high  byte
            BPL   NoNneg      ; if +ve don't negate it
                              ; else do a two's complement
            EOR   #$FF        ; invert
            SEC	            ; +1
            ADC   #$00        ; and add it

NoNneg:
            STA	Tempsq      ; save ABS(number)
            LDX	#$08        ; set bit count

Nextr2bit:
            ASL	Squarel     ; low byte *2
            ROL	Squareh     ; high byte *2+carry from low
            ASL	            ; shift number byte
            BCC	NoSqadd     ; don't do add if C = 0
            TAY               ; save A
            CLC               ; clear carry for add
            LDA	Tempsq      ; get number
            ADC	Squarel     ; add number^2 low byte
            STA	Squarel     ; save number^2 low byte
            LDA	#$00        ; clear A
            ADC	Squareh     ; add number^2 high byte
            STA	Squareh     ; save number^2 high byte
            TYA               ; get A back

NoSqadd:
            DEX               ; decrement bit count
            BNE	Nextr2bit   ; go do next bit
            RTS


Tempsq:     DS.B 0   ; temp byte for intermediate result
Numberl:    DC.B     ; number to square low byte
Numberh:    DC.B	   ; number to square high byte

Squarel:    DC.B     ; square low byte
Squareh:    DC.B	   ; square high byte
