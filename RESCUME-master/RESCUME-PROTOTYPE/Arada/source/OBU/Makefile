#
# 'make depend' uses makedepend to automatically generate dependencies 
#               (dependencies are added to end of Makefile)
# 'make'        build executable file 'mycc'
# 'make clean'  removes all .o and executable files
#


# define the executable file 
TARGET = rescume

#INPUT DIRECTORIES
DIR_PROJECT = .
DIR_INCLUDE = $(DIR_PROJECT)/.
DIR_SRC = $(DIR_PROJECT)/.
DIR_VERSION = $(DIR_PROJECT)/version

#OUTPUT DIRECTORIES
DIR_BIN = ../../bin/OBU_tchain2.0
DIR_OBJ = ../../obj/OBU_tchain2.0


FILE_CONFIG = $(DIR_PROJECT)/resources/configuration.txt
FILE_BUILD = $(DIR_PROJECT)/resources/build.txt

#LIBRARY LOCATIONS
DIR_LIBARADA = ../../external/locomate-2.0-mips_2.0.0.7
DIR_LIBASN = ../../external/asn1c
DIR_LIBPROJ = ../../external/proj-4.8.0/arada_2.0
DIR_LIBCJSON = ../../external/cJSON-2013.08.19/arada_2.0
DIR_LIBNMEA = ../../external/nmealib-0.5.3/arada_2.0

#DIR_LIBARADA = ../../../../INFLO/Arada/external/locomate-2.0-mips_2.0.0.7
#DIR_LIBASN = ../../../../INFLO/Arada/external/asn1c
#DIR_LIBPROJ = .../../../../INFLO/Arada/external/proj-4.8.0/arada_2.0
#DIR_LIBCJSON = ../../../../INFLO/Arada/external/cJSON-2013.08.19/arada_2.0
#DIR_LIBNMEA = ../../../../INFLO/Arada/external/nmealib-0.5.3/arada_2.0

#TOOLCHAIN
CC=mips-linux-uclibc-gcc
DIR_TOOL=/opt/buildroot-2011.08-rc1/output/host/usr/bin
export PATH:=$(DIR_TOOL):${PATH}

# define any compile-time flags
CFLAGS = -Wall -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW -DLOCOMATE_ME

INCLUDES+= -isystem ${DIR_LIBARADA}/incs
INCLUDES+= -isystem ${DIR_LIBARADA}/socket-CAN/include
INCLUDES+= -isystem ${DIR_LIBARADA}/src
INCLUDES+= -isystem $(DIR_LIBASN)
INCLUDES+= -I ${DIR_LIBPROJ}/include
INCLUDES+= -I ${DIR_LIBCJSON}/include
INCLUDES+= -I ${DIR_LIBNMEA}/include
INCLUDES+= -I ${DIR_INCLUDE}

LFLAGS = -L ${DIR_LIBARADA}/lib -L ${DIR_LIBPROJ}/lib -L ${DIR_LIBCJSON}/lib -L ${DIR_LIBNMEA}/lib

LIBS = -Wl,-Bstatic -lproj4.8.0 -lcjson2013.08.19 -lnmealib0.5.3 -Wl,-Bdynamic -lm -lpthread -lwave -lbluetooth -lwave-encdec

SRCS = $(patsubst $(DIR_SRC)/%.c, $(DIR_OBJ)/%.o, $(wildcard $(DIR_SRC)/*.c))
OBJS = $(SRCS:.c=.o)

FULLTARGET = $(DIR_BIN)/$(TARGET)

#VERSION STUFF
export VER_REPO:=$(shell svnversion -n ${project_loc})
VER_MAJOR:=$(shell cat $(DIR_VERSION)/ver_major)
VER_MINOR:=$(shell cat $(DIR_VERSION)/ver_minor)
VER_BUILD:=$(shell cat $(DIR_VERSION)/ver_build)
export VER_VER:=$(VER_MAJOR).$(VER_MINOR).$(VER_BUILD)
export VER_DATE:=$(shell date +'%Y%m%d')

CFLAGS+= -DVERSION='"$(VER_VER)"' -DVERSION_DATE='"$(VER_DATE)"' -DVERSION_REPO='"$(VER_REPO)"'

export VEHICLEID=$(shell awk 'BEGIN{srand();printf("0x%04x%04x", 65536*rand(), 65536*rand())}')
export VEHICLEID=0x0


.PHONY: clean

all:    $(FULLTARGET)
	@echo "\n\n"
	@echo "_______________________________ BUILD COMPLETE!! _______________________________\n"
	@echo "                   Target at: $(FULLTARGET)\n"
	@echo "               Build Version: $(VER_VER)"
	@echo "                  Build Date: $(VER_DATE)"
	@echo "          Repository Version: $(VER_REPO)\n"
	@echo "  use \"make install REMOTE=X.X.X.X\" to copy executable to remote device\n\n"
	@cp $(FILE_BUILD).bak $(FILE_BUILD)
	@sed -i -e "s/{@version}/$$VER_VER/g" $(FILE_BUILD)
	@sed -i -e "s/{@version_date}/$$VER_DATE/g" $(FILE_BUILD)
	@sed -i -e "s/{@version_repo}/$$VER_REPO/g" $(FILE_BUILD)

$(OBJS): | $(DIR_OBJ)

$(DIR_OBJ):
	mkdir -p $(DIR_OBJ)

$(DIR_OBJ)/%.o: $(DIR_SRC)/%.c
	@echo -e '\n\n_______________________________ Building: $< _______________________________'
	@$(CC) $(CFLAGS) $(INCLUDES) -c $< -o $@

$(FULLTARGET): $(OBJS) 
	@echo -e '\n\n_______________________________ Building and Linking Executable _______________________________'
	@mkdir -p $(DIR_BIN)
	$(CC) $(CFLAGS) $(OBJS) $(INCLUDES) $(LFLAGS) $(LIBS) -o $(FULLTARGET)

configuration:
	cp $(FILE_CONFIG).bak $(FILE_CONFIG)
	sed -i -e "s/{@vehicleid}/$$VEHICLEID/g" $(FILE_CONFIG)

responder: configuration
	sed -i -e 's/{@appmode}/responder/g' $(FILE_CONFIG)

oncoming: configuration
	sed -i -e 's/{@appmode}/oncoming/g' $(FILE_CONFIG)

install:
	-sshpass -p password ssh -oStrictHostKeyChecking=no root@$(REMOTE) "mkdir -p /var/bin; killall $(TARGET);"
	sshpass -p password scp -oStrictHostKeyChecking=no $(FULLTARGET) root@$(REMOTE):/var/bin/$(TARGET)
#	sshpass -p password scp -oStrictHostKeyChecking=no configuration.txt root@$(REMOTE):/var/bin/configuration.txt
	sshpass -p password scp -oStrictHostKeyChecking=no -r ./resources/* root@$(REMOTE):/var/bin/.
	sshpass -p password ssh -oStrictHostKeyChecking=no root@$(REMOTE)

clean:
	$(RM) $(FULLTARGET)
	$(RM) -r $(DIR_OBJ)
	$(RM) $(FILE_CONFIG)
	$(RM) $(FILE_BUILD)

ver-major:
	@echo $$(($$(cat $(DIR_VERSION)/ver_major) + 1)) > $(DIR_VERSION)/ver_major
	@echo 0 > $(DIR_VERSION)/ver_minor
	@echo 0 > $(DIR_VERSION)/ver_build

ver-minor:
	@echo $$(($$(cat $(DIR_VERSION)/ver_minor) + 1)) > $(DIR_VERSION)/ver_minor
	@echo 0 > $(DIR_VERSION)/ver_build

ver-build:
	@echo $$(($$(cat $(DIR_VERSION)/ver_build) + 1)) > $(DIR_VERSION)/ver_build

