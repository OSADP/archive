#
# 'make depend' uses makedepend to automatically generate dependencies 
#               (dependencies are added to end of Makefile)
# 'make'        build executable file 'mycc'
# 'make clean'  removes all .o and executable files
#


# define the executable file 
TARGET = libproj4.8.0
INCLUDEDIR = ./include
SRCDIR = ./source

# define the C compiler to use
CC_1_42=/opt/build/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin/mips-linux-uclibc-gcc
AR_1_42=/opt/build/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin/mips-linux-uclibc-ar

CC_2_0=/opt/buildroot-2011.08-rc1/output/host/usr/bin/mips-linux-uclibc-gcc
AR_2_0=/opt/buildroot-2011.08-rc1/output/host/usr/bin/mips-unknown-linux-uclibc-ar


# define any compile-time flags
CFLAGS = -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW

# define any directories containing header files other than /usr/include
#
INCLUDES = -I $(INCLUDEDIR)

LIBS = -Wl,-Bdynamic -lm 

OUTDIR_1_42 = ./arada_1.42
OBJDIR_1_42 = ./obj/1_42
SRCS_1_42 = $(patsubst $(SRCDIR)/%.c, $(OBJDIR_1_42)/%.o, $(wildcard $(SRCDIR)/*.c))
OBJS_1_42 = $(SRCS_1_42:.c=.o)

OUTDIR_2_0 = ./arada_2.0
OBJDIR_2_0 = ./obj/2_0
SRCS_2_0 = $(patsubst $(SRCDIR)/%.c, $(OBJDIR_2_0)/%.o, $(wildcard $(SRCDIR)/*.c))
OBJS_2_0 = $(SRCS_2_0:.c=.o)



FULLTARGET_1_42 = $(OUTDIR_1_42)/lib/$(TARGET)
FULLTARGET_2_0 = $(OUTDIR_2_0)/lib/$(TARGET)



.PHONY: clean

all: $(FULLTARGET_1_42) $(FULLTARGET_2_0)
	@echo -e '\n\nBUILD COMPLETE!!\n\n'

$(OBJS_1_42): | $(OBJDIR_1_42)

$(OBJDIR_1_42):
	mkdir -p $(OBJDIR_1_42)

$(OBJDIR_1_42)/%.o: $(SRCDIR)/%.c
	$(CC_1_42) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET_1_42): $(OBJS_1_42)
	mkdir -p $(OUTDIR_1_42)/lib
	mkdir -p $(OUTDIR_1_42)/include
	$(AR_1_42) rcs $(FULLTARGET_1_42).a $(OBJS_1_42)
	cp $(INCLUDEDIR)/* $(OUTDIR_1_42)/include/.


$(OBJS_2_0): | $(OBJDIR_2_0)

$(OBJDIR_2_0):
	mkdir -p $(OBJDIR_2_0)

$(OBJDIR_2_0)/%.o: $(SRCDIR)/%.c
	$(CC_2_0) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET_2_0): $(OBJS_2_0)
	mkdir -p $(OUTDIR_2_0)/lib
	mkdir -p $(OUTDIR_2_0)/include
	$(AR_2_0) rcs $(FULLTARGET_2_0).a $(OBJS_2_0)
	cp $(INCLUDEDIR)/* $(OUTDIR_2_0)/include/.


clean:
	$(RM) -r $(OUTDIR_1_42) $(OUTDIR_2_0) 
	$(RM) -r $(OBJDIR_1_42) $(OBJDIR_2_0) 


