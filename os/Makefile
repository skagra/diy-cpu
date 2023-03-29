include ../build-conf/vars.mk
include ../build-conf/rules.mk

TARGETS = $(BIN)/os.bin 

all: $(BIN) all-asm

bin: 
	mkdir $(BIN)

all-asm: $(TARGETS)
	
clean:
	rmdir /S /Q $(BIN)
