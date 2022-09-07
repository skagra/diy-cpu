include ../../build-conf/vars.mk
include ../../build-conf/rules.mk

all: ucode

.PHONY: ucode

ucode: 
	dotnet run --project ../../microasm/microasm.csproj 4 1 1 1 . bin
