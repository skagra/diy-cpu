$(BIN)/%.bin $(BIN)/%.asm : %.asm
	$(ASM) $< -f3 -o$(@D)/$(*F).bin -l$(@D)/$(*F).asm