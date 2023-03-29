include ../build-conf/vars.mk
include ../build-conf/rules.mk

TARGETS = $(BIN) $(BIN)/tests.bin 

all: all-asm

bin: 
	mkdir $(BIN)

all-asm: $(TARGETS)
	
clean:
	rmdir /S /Q $(BIN)