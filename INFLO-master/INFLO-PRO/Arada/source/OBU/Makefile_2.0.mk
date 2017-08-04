#
# 'make depend' uses makedepend to automatically generate dependencies 
#               (dependencies are added to end of Makefile)
# 'make'        build executable file 'mycc'
# 'make clean'  removes all .o and executable files
#


# define the executable file 
TARGET = infloOBU
BINDIR = ../../bin/OBU_tchain2.0
OBJDIR = ../../obj/OBU_tchain2.0
INCLUDEDIR = .
SRCDIR = .
FILE_CONFIG = config-file.txt

ARADADIR = ../../external/locomate-2.0-mips_2.0.0.7
ASNDIR = ../../external/asn1c
PROJDIR = ../../external/proj-4.8.0/arada_2.0

CC=mips-linux-uclibc-gcc
TOOLDIR=/opt/buildroot-2011.08-rc1/output/host/usr/bin
export PATH:=$(TOOLDIR):${PATH}


# define any compile-time flags
CFLAGS = -w -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW

INCLUDES+= -I .
INCLUDES+= -I ${ARADADIR}/incs
INCLUDES+= -I ${ARADADIR}/socket-CAN/include
INCLUDES+= -I ${ARADADIR}/src
INCLUDES+= -I $(ASNDIR)
INCLUDES+= -I ${PROJDIR}/include


LFLAGS = -L ${ARADADIR}/lib -L ${PROJDIR}/lib

LIBS = -Wl,-Bstatic -lproj4.8.0 -Wl,-Bdynamic -lm -lpthread -lwave -lbluetooth -lwave-encdec

export VEHICLEID=$(shell awk 'BEGIN{srand();printf("0x%04x%04x", 65536*rand(), 65536*rand())}')
#export VEHICLEID=0x0

SRCS = $(patsubst $(SRCDIR)/%.c, $(OBJDIR)/%.o, $(wildcard $(SRCDIR)/*.c))
OBJS = $(SRCS:.c=.o)

FULLTARGET = $(BINDIR)/$(TARGET)


.PHONY: clean


all:    $(FULLTARGET)
	@echo -e '\n\nBUILD COMPLETE!!\nTarget at: $(FULLTARGET)\n'
	@echo -e 'use "make install REMOTE=X.X.X.X to copy executable to remote device\n\n'

$(OBJS): | $(OBJDIR)

$(OBJDIR):
	mkdir -p $(OBJDIR)

$(OBJDIR)/%.o: $(SRCDIR)/%.c
	$(CC) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET): $(OBJS) 
	mkdir -p $(BINDIR)
	$(CC) $(CFLAGS) $(OBJS) $(INCLUDES) $(LFLAGS) $(LIBS) -o $(FULLTARGET)

configuration:
	cp $(FILE_CONFIG).bak $(FILE_CONFIG)
	sed -i -e "s/{@vehicleid}/$$VEHICLEID/g" $(FILE_CONFIG)

install:
	-sshpass -p B@ttelle ssh -oStrictHostKeyChecking=no root@$(REMOTE) "mkdir -p /var/bin; mkdir -p /var/bin/maps; mkdir -p /var/bin/spoofs; killall -9 $(TARGET);"
	sshpass -p B@ttelle scp -oStrictHostKeyChecking=no $(FULLTARGET) root@$(REMOTE):/var/bin/$(TARGET)
	sshpass -p B@ttelle scp -oStrictHostKeyChecking=no config-file.txt root@$(REMOTE):/var/bin/config-file.txt
	sshpass -p B@ttelle scp -oStrictHostKeyChecking=no -r maps/ root@$(REMOTE):/var/bin
	sshpass -p B@ttelle scp -oStrictHostKeyChecking=no -r spoofs/ root@$(REMOTE):/var/bin
	sshpass -p B@ttelle ssh -oStrictHostKeyChecking=no root@$(REMOTE)

clean:
	$(RM) $(FULLTARGET)
	$(RM) -r $(OBJDIR)


