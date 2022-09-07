$(BIN)/%.bin $(BIN)/%.txt : %.asm
	$(ASM) $< -f3 -o$(@D)/$(*F).bin -l$(@D)/$(*F).txt