SUBDIRS = 

TOPTARGETS := all clean

SUBDIRS := examples os tests ucode

$(TOPTARGETS): $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ $(MAKECMDGOALS)
	
.PHONY: $(TOPTARGETS) $(SUBDIRS)
	
