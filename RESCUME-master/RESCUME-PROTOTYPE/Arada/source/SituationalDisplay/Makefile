#
# 'make depend' uses makedepend to automatically generate dependencies 
#               (dependencies are added to end of Makefile)
# 'make'        build executable file 'mycc'
# 'make clean'  removes all .o and executable files
#


# define the executable file 
TARGET = situational
BINDIR = ../../bin/OBU_tchain2.0
OBJDIR = ../../obj/OBU_tchain2.0
INCLUDEDIR = .
SRCDIR = .

ARADADIR = ../../external/locomate-2.0-mips_2.0.0.7
ASNDIR = ../../external/asn1c
PROJDIR = ../../external/proj-4.8.0/arada_2.0
CJSONDIR = ../../external/cJSON-2013.08.19/arada_2.0
NMEALIBDIR = ../../external/nmealib-0.5.3/arada_2.0

CC=mips-linux-uclibc-gcc
TOOLDIR=/opt/buildroot-2011.08-rc1/output/host/usr/bin
export PATH:=$(TOOLDIR):${PATH}


# define any compile-time flags
CFLAGS = -Wall -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW -DLOCOMATE_ME

INCLUDES+= -isystem ${ARADADIR}/incs
INCLUDES+= -isystem ${ARADADIR}/socket-CAN/include
INCLUDES+= -isystem ${ARADADIR}/src
INCLUDES+= -isystem $(ASNDIR)
INCLUDES+= -I ${INCLUDEDIR}
INCLUDES+= -I ${PROJDIR}/include
INCLUDES+= -I ${CJSONDIR}/include
INCLUDES+= -I ${NMEALIBDIR}/include


LFLAGS = -L ${ARADADIR}/lib -L ${PROJDIR}/lib -L ${CJSONDIR}/lib -L ${NMEALIBDIR}/lib

LIBS = -Wl,-Bstatic -lproj4.8.0 -lcjson2013.08.19 -lnmealib0.5.3 -Wl,-Bdynamic -lm -lpthread -lwave -lbluetooth -lwave-encdec


SRCS = $(patsubst $(SRCDIR)/%.c, $(OBJDIR)/%.o, $(wildcard $(SRCDIR)/*.c))
OBJS = $(SRCS:.c=.o)

FULLTARGET = $(BINDIR)/$(TARGET)

export VEHICLEID=$(shell awk 'BEGIN{srand();printf("0x%04x%04x", 65536*rand(), 65536*rand())}')


.PHONY: clean

all:    $(FULLTARGET)
	@echo -e '\n\nBUILD COMPLETE!!\nTarget at: $(FULLTARGET)\n'
	@echo -e 'use "make install REMOTE=X.X.X.X to copy executable to remote device\n\n'

$(OBJS): | $(OBJDIR)

$(OBJDIR):
	mkdir -p $(OBJDIR)

$(OBJDIR)/%.o: $(SRCDIR)/%.c
	@echo -e '\n\n_______________________________ Building: $< _______________________________'
	@$(CC) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET): $(OBJS) 
	@echo -e '\n\n_______________________________ Building and Linking Executable _______________________________'
	@mkdir -p $(BINDIR)
	$(CC) $(CFLAGS) $(OBJS) $(INCLUDES) $(LFLAGS) $(LIBS) -o $(FULLTARGET)

install:
	-sshpass -p password ssh -oStrictHostKeyChecking=no root@$(REMOTE) "mkdir -p /var/bin; killall $(TARGET);"
	sshpass -p password scp -oStrictHostKeyChecking=no $(FULLTARGET) root@$(REMOTE):/var/bin/$(TARGET)
	sshpass -p password ssh -oStrictHostKeyChecking=no root@$(REMOTE)

clean:
	$(RM) $(FULLTARGET)
	$(RM) -r $(OBJDIR)


