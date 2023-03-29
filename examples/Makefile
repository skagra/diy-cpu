include ../build-conf/vars.mk
include ../build-conf/rules.mk

TARGETS = $(BIN)/mul10.bin $(BIN)/multiply.bin $(BIN)/RAM.bin $(BIN)/square.bin $(BIN)/dow.bin

all: $(BIN) all-asm

bin: 
	mkdir $(BIN)

all-asm: $(TARGETS)
	
clean:
	rmdir /S /Q $(BIN)
	