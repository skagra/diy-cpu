include ../../build-conf/vars.mk
include ../../build-conf/rules.mk

all: bin ucode

.PHONY: ucode

bin: 
	mkdir $(BIN)

ucode: 
	dotnet run --project ../../microasm/microasm.csproj 4 1 1 1 . bin
