#
# 'make depend' uses makedepend to automatically generate dependencies 
#               (dependencies are added to end of Makefile)
# 'make'        build executable file 'mycc'
# 'make clean'  removes all .o and executable files
#


# define the executable file 
TARGET = infloRSU
BINDIR = ../../bin/RSU_tchain1.42
OBJDIR = ../../obj/RSU_tchain1.42
INCLUDEDIR = .
SRCDIR = .

ARADADIR = ../../external/locomate-mips_1.86.4
ASNDIR = ../../external/asn1c
CURLDIR = ../../external/curl-7.35.0/arada_1.42


# define the C compiler to use
CC=mips-linux-uclibc-gcc
TOOLDIR=/opt/build/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin
export PATH:=$(TOOLDIR):${PATH}


# define any compile-time flags
CFLAGS = -w -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW

# define any directories containing header files other than /usr/include
#
INCLUDES = -I $(INCLUDEDIR)
INCLUDES+= -I ${ARADADIR}/incs
INCLUDES+= -I ${ARADADIR}/socket-CAN/include
INCLUDES+= -I ${ARADADIR}/src
INCLUDES+= -I $(ASNDIR)
INCLUDES+= -I $(CURLDIR)/include


# define library paths in addition to /usr/lib
#   if I wanted to include libraries not in /usr/lib I'd specify
#   their path using -Lpath, something like:
LFLAGS = -L ${ARADADIR}/lib -L $(ASNDIR) -L $(CURLDIR)/lib

# define any libraries to link into executable:
#   if I want to link in libraries (libx.so or libx.a) I use the -llibname 
#   option, something like (this will link in libmylib.so and libm.so:
LIBS = -Wl,-Bstatic -lcurl -Wl,-Bdynamic -lm -lpthread -lwave -lbluetooth -lwave-encdec #-Xlinker -rpath -Xlinker $(ARADADIR)/lib 


# define the C source files
SRCS = $(patsubst $(SRCDIR)/%.c, $(OBJDIR)/%.o, $(wildcard $(SRCDIR)/*.c))

# define the C object files 
#
# This uses Suffix Replacement within a macro:
#   $(name:string1=string2)
#         For each word in 'name' replace 'string1' with 'string2'
# Below we are replacing the suffix .c of all words in the macro SRCS
# with the .o suffix
#
OBJS = $(SRCS:.c=.o)

FULLTARGET = $(BINDIR)/$(TARGET)


.PHONY: clean

all:    $(FULLTARGET)
	@echo -e '\n\nBUILD COMPLETE!!\nTarget at: $(FULLTARGET)\n'
	@echo -e 'use "make install REMOTE=X.X.X.X" to copy executable to remote device\n\n'

$(OBJS): | $(OBJDIR)

$(OBJDIR):
	mkdir -p $(OBJDIR)

$(OBJDIR)/%.o: $(SRCDIR)/%.c
	$(CC) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET): $(OBJS) 
	mkdir -p $(BINDIR)
	$(CC) $(CFLAGS) $(OBJS) $(INCLUDES) $(LFLAGS) $(LIBS) -o $(FULLTARGET)

install:
	sshpass -p password scp $(FULLTARGET) root@$(REMOTE):/var/bin/$(TARGET)
	sshpass -p password scp config-file.txt root@$(REMOTE):/var/bin/config-file.txt
	sshpass -p password ssh root@$(REMOTE)

clean:
	$(RM) $(FULLTARGET)
	$(RM) -r $(OBJDIR)


