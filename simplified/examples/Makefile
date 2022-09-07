include ../../build-conf/vars.mk
include ../../build-conf/rules.mk

TARGETS = bin/simple.bin

all: all-asm

all-asm: bin $(TARGETS)

bin: 
	mkdir $(BIN)

clean:
	rmdir /S /Q $(BIN)