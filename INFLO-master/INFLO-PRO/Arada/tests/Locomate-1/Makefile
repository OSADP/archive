# Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

# Proprietary and Confidential Material.

#TOOLCHAIN=/opt/build/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin
#TOOLCHAIN=/opt/build-1.42/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin
TOOLCHAIN=/opt/buildroot-2011.08-rc1/output/host/usr/bin

export PATH:=$(TOOLCHAIN):/${PATH}

INCS+= -I ../incs
INCS+= -I ../socket-CAN/include

INCS+= -I .

BINDIR= ../bin

LIBDIR= ../lib

#CC=mips-linux-uclibc-gcc
CC=mips-linux-gcc

ALL= bt_server bt_client bt_clientserver


all:	${ALL}
	mv -f ${ALL} ../bin
	cp -f ../bin/bt_server ../bin/bt_client ../bin/bt_clientserver /tftp
	
%.o: %.c
	$(CC) $(INCS) -c -DLOCOMATE -DMAX_NUM_APPS=125 -DSDK_NEW -o $@ $<

#LDFLAGS= -lm -lpthread -lbluetooth -L $(LIBDIR) -Xlinker -rpath -Xlinker $(LIBDIR) -lwave-encdec -Lasn1c/
LDFLAGS= -lm -lpthread -lbluetooth -L $(LIBDIR) -Xlinker -rpath -Xlinker $(LIBDIR) -Lasn1c/

bt_server: bt_server.o
	${CC} -g  ${LDFLAGS} -o bt_server bt_server.o

bt_client: bt_client.o
	${CC} -g  ${LDFLAGS} -o bt_client bt_client.o

bt_clientserver: bt_clientserver.o server_thread.o client_thread.o
	${CC} -g ${LDFLAGS} -o bt_clientserver bt_clientserver.o server_thread.o client_thread.o

#server_thread: server_thread.o
#	${CC} -g ${LDFLAGS} -o server_thread server_thread.o
#	
#client_thread: client_thread.o
#	${CC} -g ${LDFLAGS} -o client_thread client_thread.o

clean:
	rm -f *.o
	rm -f ${BINDIR}/*	
