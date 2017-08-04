# Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

# Proprietary and Confidential Material.

#TOOLCHAIN=/opt/build/toolchain/PB44/buildroot/build_mips/staging_dir/usr/bin
#CC=mips-linux-uclibc-gcc
include ./makefile_tlcn.inc

export PATH:=$(TOOLCHAIN):/${PATH}
OBJDIR=${PWD}/..
INCS+= -I ${OBJDIR}/incs
INCS+= -I ${OBJDIR}/socket-CAN/include

INCS+= -I ${OBJDIR}/src

BINDIR= ${OBJDIR}/bin

LIBDIR= ${OBJDIR}/lib


ALL= wsmpdemo xmitgpswave logxmitgps remotetx ipudplogger wlogger remoterx localtx localtx_sec localgpsrx localrx localrx_sec bluetoothrx bluetoothtx hciconfig csvparser jitter getwrssrx getwbsstxrx getwbsstxrxencdec remotetxrx localrxbsm localtxbsm localtxrsa localtxpvd listdevs


all:	${ALL}
	make     -C ${OBJDIR}/src/can-utils/ PREFIX=${OBJDIR}   CFLAGS="-O2 -Wall -Wno-parentheses\
        -fno-strict-aliasing \
        -DSO_RXQ_OVFL=40 \
        -DPF_CAN=29 \
        -DAF_CAN=PF_CAN \
        ${INCS}" CC=mips-linux-uclibc-gcc
	make    PREFIX=${OBJDIR} -C ${OBJDIR}/src/can-utils/ install
	mv -f ${ALL} ${OBJDIR}/bin

%.o: %.c
	$(CC) $(INCS) -c -DLOCOMATE -DSDK_NEW -o $@ $<

LDFLAGS= -lm -lpthread -lwave -lbluetooth -L $(LIBDIR) -Xlinker -rpath -Xlinker $(LIBDIR) -lwave-encdec -Lasn1c/


wsmpdemo: wsmpdemo.o  wavelogger.o wavegps.o gpsc_probe.o asnlogger.o
	${CC} -g   $(LDFLAGS) -o wsmpdemo  wsmpdemo.o  wavelogger.o wavegps.o gpsc_probe.o asnlogger.o

xmitgpswave: wavedemoTX.o wavegps.o gpsc_probe.o
	${CC} -g  $(LDFLAGS) -o xmitgpswave  wavedemoTX.o  wavegps.o gpsc_probe.o

logxmitgps: logxmitgps.o wavelogger.o wavegps.o gpsc_probe.o
	${CC} -g  ${LDFLAGS} -o logxmitgps logxmitgps.o wavelogger.o wavegps.o gpsc_probe.o

remotetx: remoteautowbsstx.o
	${CC} -g  ${LDFLAGS} -o remotetx remoteautowbsstx.o

remoterx: remoteautowbssrx.o
	${CC} -g ${LDFLAGS} -o remoterx remoteautowbssrx.o

remotetxrx: remoteautowbss_tx_rx_wrss.o
	${CC} ${LDFLAGS} -o remotetxrx remoteautowbss_tx_rx_wrss.o

localtx: autowbsstx.o 
	${CC} -g  ${LDFLAGS} -o localtx autowbsstx.o

localtxbsm: autowbsstxbsm.o 
	${CC} -g  ${LDFLAGS} -o localtxbsm autowbsstxbsm.o

localtxrsa: autowbsstxrsa.o 
	${CC} -g  ${LDFLAGS} -o localtxrsa autowbsstxrsa.o

localtxpvd: autowbsstxpvd.o 
	${CC} -g  ${LDFLAGS} -o localtxpvd autowbsstxpvd.o

localgpsrx: autowbssgpsrx.o wavegps.o gpsc_probe.o
	${CC} ${LDFLAGS} -o localgpsrx autowbssgpsrx.o wavegps.o gpsc_probe.o

localrx: autowbssrx.o
	${CC} ${LDFLAGS} -o localrx autowbssrx.o wavegps.o gpsc_probe.o

localrxbsm: autowbssrxbsm.o
	${CC} ${LDFLAGS} -o localrxbsm autowbssrxbsm.o wavegps.o gpsc_probe.o

localtx_sec: autowbsstxsec.o UserApi.o msg.o 
	${CC} -g  ${LDFLAGS} -o localtx_sec autowbsstxsec.o UserApi.o msg.o 

localrx_sec: autowbssrxsec.o UserApi.o msg.o
	${CC} -g   ${LDFLAGS} -o localrx_sec autowbssrxsec.o UserApi.o msg.o 

bluetoothtx: bluetoothtx.o 
	${CC} -g  ${LDFLAGS} -o bluetoothtx  bluetoothtx.o

bluetoothrx: bluetoothrx.o bluetooth_asd.o
	${CC} -g  ${LDFLAGS} -o bluetoothrx  bluetoothrx.o bluetooth_asd.o

#bluetooth_asd: bluetooth_asd.o 
#	${CC} -g  ${LDFLAGS} -o bluetooth_asd  bluetooth_asd.o

hciconfig: hciconfig.o 
	${CC} -g  ${LDFLAGS} -o hciconfig  hciconfig.o

getwrssrx: autowbsswrss.o
	${CC} ${LDFLAGS} -o getwrssrx autowbsswrss.o

getwbsstxrx: autowbss_tx_rx_wrss.o
	${CC} -g   ${LDFLAGS} -o getwbsstxrx autowbss_tx_rx_wrss.o 

getwbsstxrxencdec: autowbss_tx_rx_encdec.o UserApi.o msg.o wavegps.o wavelogger.o can_data.o gpsc_probe.o fileparser.o asnlogger.o cb_cnt.o live_parser.o LocoParser.o bluetooth_asd.o wnodehash.o
	${CC} -g   ${LDFLAGS} -I ./asn1c  -o getwbsstxrxencdec autowbss_tx_rx_encdec.o UserApi.o msg.o wavegps.o wavelogger.o can_data.o gpsc_probe.o fileparser.o asnlogger.o cb_cnt.o live_parser.o LocoParser.o bluetooth_asd.o wnodehash.o

ipudplogger: ipudplogger.o wavelogger.o wavegps.o gpsc_probe.o
	${CC} ${LDFLAGS} -o ipudplogger ipudplogger.o wavelogger.o wavegps.o gpsc_probe.o

wlogger: wlogger.o wavelogger.o wavegps.o gpsc_probe.o
	${CC} ${LDFLAGS} -o wlogger wlogger.o wavelogger.o wavegps.o gpsc_probe.o

csvparser: csvparser.o 
	${CC} ${LDFLAGS} -o csvparser csvparser.o

jitter: jitter.o 
	${CC} ${LDFLAGS} -lm -o jitter jitter.o

listdevs: listdevs.o
	${CC} ${LDFLAGS} -o listdevs listdevs.o  -lusb-1.0

clean:
	rm -f *.o
	make -C ${OBJDIR}/src/can-utils/ clean
	rm -f ${BINDIR}/*	
