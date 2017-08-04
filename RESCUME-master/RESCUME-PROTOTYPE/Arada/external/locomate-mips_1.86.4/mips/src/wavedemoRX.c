/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include <stdio.h>
#include <ctype.h>
#include <termio.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <time.h>
#include <signal.h>
#include "wave.h"

/*Sample Application:  Demonstrate how to receive WSMP packets from the local device */
int main()
{
	USTEntry entry;
	WMEApplicationRequest req; 
	WSMIndication pkt;
	int i, attempts = 10, drops = 0;
	int pid, ret = 0;
	
	pid = getpid();
	memset(&pkt.data, 0, sizeof(WSMData) );
	
	/*Build the UST entry; ACID-ACM combination should be unique per USER or PROVIDER*/
	entry.acid = 1;
	strcpy(entry.acm.contents, "shubham");
	entry.acm.length = strlen(entry.acm.contents);
	
	/*Open the WAVE DEVICE on THIS Machine (WAVEDEVICE_LOCAL), invokeWAVEDevice(WAVEDEVICE_LOCAL, blockflag)*/
	/*The second argument is a block flag that speicifies whether Read/Write are Nonblocking(0) or Blocking(1)*/
	/*It is ignored if WAVEDEVICE_REMOTE is given as first argument to the invokeWAVEDEVICE(...) call */
	
	ret = invokeWAVEDevice(WAVEDEVICE_LOCAL, 0);
	printf("Invoking  WAVE driver [%d]\n",ret);
	
	/*Register this USER application with the WME*/
	ret = registerUser(pid, &entry);
	printf("Registering as a USER [%d] ACID =%d ACM = %s \n", ret, entry.acid, entry.acm.contents );
	
	
	while(1)  {
		/*Read the Packet from device*/
		ret = rxWSMPacket(pid, &pkt);
		if (ret >= 0) {
			/*Packet is recived in pkt*/
			printf("RXWSM Packet rateindex=%d, txpower=%d\n", pkt.chaninfo.rate, pkt.chaninfo.txpower);
		}
		ret = 0;			
	}
	/*ON EXIT do removeUser(pid, &entry), so that next time you launch this application your registration succeds*/
	
}
