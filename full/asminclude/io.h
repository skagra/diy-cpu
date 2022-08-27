; Memory mapped I/O addresses

POSTL           = $4000 ; POST low byte Hex LCD
POSTH           = $4001 ; POST high byte Hex LCD
TERMINAL        = $4002 ; Write to the built in terminal
KEYBOARD        = $4003 ; Read from the built in keyboard input
KEYBOARDRDY     = $4004 ; Check whether keyboard has characters waiting
TELNETOUT       = $4005 ; Write to the telnet connection
TELNETIN        = $4006 ; Read from the telnet connection
TELNETRDY       = $4007 ; Check whether telnet has characters waiting
