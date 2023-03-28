SUBDIRS = examples os tests ucode

all: subdirs

.PHONY: subdirs $(SUBDIRS)

subdirs: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@
	
