include ../../build-conf/vars.mk
include ../../build-conf/rules.mk

all: bin ucode

bin: 
	mkdir $(BIN)

ucode: $(BIN)/mModeDecoder.bin

$(BIN)/mModeDecoder.bin: mModeDecoder.txt mOpDecoder.txt uCode.txt uCtrl.txt uOps.txt
	dotnet run --project ../../microasm/microasm.csproj 4 1 1 1 . bin >$(BIN)/log.txt
