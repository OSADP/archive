/**
 * @file         BtServer.c
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2014 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */

#include "BtServer.h"

#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <arpa/inet.h>
#include <bluetooth/bluetooth.h>
#include <bluetooth/hci.h>
#include <bluetooth/hci_lib.h>
#include <bluetooth/sdp.h>
#include <bluetooth/sdp_lib.h>
#include <bluetooth/rfcomm.h>

#include "Debug.h"

#define RFCOMM_CHAN 13

void btServer_initAdapter();
void btServer_registerSdp();
void btServer_closeSdp();
void *btServer_serverStart(void *arg);
void btServer_handleMessage(char *msg);
void *btServer_clientRun(void *arg);
void btServer_destroyClients();
int btServer_str2uuid(const char *uuid_str, uuid_t *uuid);

static sdp_session_t *sdpSession = 0;
static pthread_t btServerThread = 0;
static BtClient btClients[MAX_CONNECTIONS];
static int btServerSocket = 0;
static void (*rxMsgCallback)(char *) = 0;
static pthread_mutex_t txLock = PTHREAD_MUTEX_INITIALIZER;


/***********************************************
 * EXPOSED METHODS
 **********************************************/
void btServer_start(char *uuid)
{
	btServer_registerSdp(uuid);

	if (!btServerThread)
		pthread_create(&btServerThread, NULL, btServer_serverStart, NULL);
}

void btServer_stop()
{
	if (btServerThread)
	{
		DBG_INFO(DBGM_BT, printf("Stopping...\n"));
		pthread_cancel(btServerThread);
		pthread_join(btServerThread, NULL);
		btServerThread = 0;
	}

	btServer_destroyClients();
}

void btServer_setRxMessageCallback(void (*callback)(char *))
{
	rxMsgCallback = callback;
}

void btServer_sendMessageToAllClients(char *message)
{
	pthread_mutex_lock(&txLock);

	int retvalue;
	int i;
	char start = 2;
	char end = 3;

	static uint8_t data[5000];

	int msgLength = strlen(message) + 2;

	if (msgLength > sizeof(data))
	{

	}
	else
	{
		data[0] = 2;
		memcpy(data + 1, message, msgLength - 2);
		data[msgLength - 1] = 3;
		//scroll through each connected client
		for(i=0 ;i<MAX_CONNECTIONS ;i++)
		{
			if (btClients[i].state == CONNECTED) {
				retvalue = write(btClients[i].sock, data, msgLength);
				if (retvalue < 0)
				{
					DBG_ERR(DBGM_BT, printf("Error Sending to BT Connection %d. Setting to DISCONNECTED\n", i));
					btClients[i].state = DISCONNECTED;
				}
			}
		}
	}

	pthread_mutex_unlock(&txLock);
}


/***********************************************
 * INTERNAL METHODS
 **********************************************/

void btServer_registerSdp(char *uuid)
{
	if (sdpSession)
		return;

	DBG_INFO(DBGM_BT, printf("Registering with UUID: %s ...\n", uuid));

	sdp_record_t *rec = sdp_record_alloc();
	uuid_t l2cap_uuid, root_uuid, rfcomm_uuid, service_uuid;
	sdp_list_t *service_class_list = 0, *l2cap_list = 0, *rfcomm_list = 0, *root_list = 0, *protocol_list = 0, *access_protocol_list = 0;
	sdp_data_t *channel = 0;
	uint8_t rfcomm_chan = RFCOMM_CHAN;

	if(!btServer_str2uuid(uuid, &service_uuid))
	{
		perror("BtServer: Invalid UUID");
		return;
  	}

	//Create record for service to broadcast (connect with smartphone)
	sdp_set_service_id(rec, service_uuid);
	service_class_list = sdp_list_append(0, &service_uuid);
	sdp_set_service_classes(rec, service_class_list);

	sdp_uuid16_create(&root_uuid, PUBLIC_BROWSE_GROUP);
	root_list = sdp_list_append(0, &root_uuid);
	sdp_set_browse_groups(rec, root_list);

	sdp_uuid16_create(&l2cap_uuid, L2CAP_UUID);
	l2cap_list = sdp_list_append(0, &l2cap_uuid);

	sdp_uuid16_create(&rfcomm_uuid, RFCOMM_UUID);
	channel = sdp_data_alloc(SDP_UINT8, &rfcomm_chan);
	rfcomm_list = sdp_list_append(0, &rfcomm_uuid);
	sdp_list_append(rfcomm_list, channel);
	protocol_list = sdp_list_append(0, rfcomm_list);

	access_protocol_list = sdp_list_append(0, protocol_list);
	sdp_set_access_protos(rec, access_protocol_list);

	sdp_set_info_attr(rec, "INC-ZONE", "Battelle", "INC-ZONE Application for RESCUME"); 

	sdpSession = sdp_connect(BDADDR_ANY, BDADDR_LOCAL, SDP_RETRY_IF_BUSY);
	if (sdpSession != NULL)
	{
		int error = sdp_record_register(sdpSession, rec, 0);
		DBG_INFO(DBGM_BT, printf("BtServer: SDP Record Registered.\n"));
	}
	else
	{
		DBG_ERR(DBGM_BT, printf("BtServer: Unable to connect to SDP Server.\n"));
	}

	//Free service lists
	sdp_data_free(channel);
	sdp_list_free(l2cap_list, 0);
	sdp_list_free(rfcomm_list, 0);
	sdp_list_free(root_list, 0);
	sdp_list_free(access_protocol_list, 0);
}

void btServer_closeSdp()
{
	if (!sdpSession)
		return;

	sdp_close(sdpSession);
	sdpSession = 0;
}

void btServer_initAdapter() {
	// Reset Bluetooth and Initialize
	char cmd[50];

	sprintf(cmd, "/usr/local/bin/hciconfig hci0 up");
	DBG_INFO(DBGM_BT, printf("BtServer: Executing command:%s\n", cmd));
	system(cmd);

	sprintf(cmd, "/usr/local/bin/hciconfig hci0 piscan");
	DBG_INFO(DBGM_BT, printf("BtServer: Executing command:%s\n", cmd));
	system(cmd);
}

void *btServer_serverStart( void *args )
{
	btServer_initAdapter();
	
	// Allocate connections
	int i;
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		btClients[i].sock = (int) NULL;
		btClients[i].state = DISCONNECTED;
	}

	// allocate socket
	DBG_INFO(DBGM_BT, printf("BtServer: Allocating Socket.\n"));
	btServerSocket = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

	struct sockaddr_rc loc_addr = { 0 };
	loc_addr.rc_family = AF_BLUETOOTH;
	loc_addr.rc_bdaddr = *BDADDR_ANY;
	loc_addr.rc_channel = RFCOMM_CHAN;

	//Bind to socket
	DBG_INFO(DBGM_BT, printf("BtServer: Binding Socket.\n"));
	if(bind(btServerSocket, (struct sockaddr *)&loc_addr, sizeof(loc_addr)))
		perror("BIND");

	// put socket into listening mode
	DBG_INFO(DBGM_BT, printf("BtServer: Listening...\n"));
	listen(btServerSocket, 1);

	while(1)
	{
		struct sockaddr_rc rem_addr = { 0 };
		socklen_t opt = sizeof(rem_addr);
		int client = accept(btServerSocket, (struct sockaddr *)&rem_addr, &opt);
		
		for(i = 0; (i < MAX_CONNECTIONS) & (btClients[i].sock != (int) NULL); i++);	//look for first available socket fd

		if(i < MAX_CONNECTIONS)
		{
			if(client >= 0)
			{
				char buf[30];
				ba2str( &rem_addr.rc_bdaddr, buf );
				DBG_INFO(DBGM_BT, printf("BtServer: Accepted connection #%d from %s\n", i, buf));

				strncpy(btClients[i].macString, buf, BT_MAC_STRING_LENGTH);
				btClients[i].sock = client;
				btClients[i].state = CONNECTED;
				pthread_create(&btClients[i].threadId, NULL, btServer_clientRun, (void *)&btClients[i]);
			}

		}
		else
		{
			DBG_ERR(DBGM_BT, printf("BtServer: Unable to accept new connection.  Max number of connections reached.\n"));
		}

		pthread_testcancel();
		sleep(1);
	}

	return 0;
}

void *btServer_clientRun(void *arg)
{
	BtClient *client = (BtClient *) arg;
	char ch[100];
	char msg[BT_MSG_LENGTH];
	int msgLength = 0;
	
	while(client->state == CONNECTED)
	{
		int retvalue = read(client->sock, &ch, sizeof(ch));
		if(retvalue < 0)	//connection has been closed
		{
			client->state = DISCONNECTED;
		} 
		else if(retvalue > 0)
		{	
			if (retvalue + msgLength >= BT_MSG_LENGTH)
			{
				msgLength = 0;
			}
			memcpy(msg + msgLength, ch, retvalue);
			msgLength += retvalue;
			msg[msgLength] = '\0';

			char *p_msg_start = strstr(msg, "\x02");
			if (p_msg_start == NULL)
			{
				msgLength = 0;
			}
			else
			{
				if(p_msg_start != msg)
				{
					int shift = (p_msg_start - msg);
					memcpy(msg, msg + shift, msgLength + 1 - shift);
					msgLength -= shift;
				}

				char *p_msg_end = strstr(msg, "\x03");
				if(p_msg_end != NULL)
				{
					*p_msg_end = '\0';
					DBG(DBGM_BT, printf("BtClient '%s': Received Message %s\n", client->macString, msg + 1));
					if (rxMsgCallback)
						rxMsgCallback(msg + 1);
					int shift = (p_msg_end - msg) + 1;
					memcpy(msg, msg + shift, msgLength + 1 - shift);
					msgLength -= shift;
					DBG(DBGM_BT, printf("BtClient '%s': New message length = %d\n", client->macString, msgLength));
				}
			}
		} 
		else 
		{
			DBG_INFO(DBGM_BT, printf("BtClient '%s': Connection Closed\n", client->macString));
			client->state = DISCONNECTED;
		}
		sleep(0);
	}

	//TODO: Shut socket down
	int retvalue = close(client->sock);
	if (retvalue < 0)
	{
		DBG_ERR(DBGM_BT, printf("BtClient '%s': Error Closing Socket...\n", client->macString));
	}
	else
	{
		DBG_INFO(DBGM_BT, printf("BtClient '%s': Bluetooth Socket Closed\n", client->macString));
	}
	client->sock = 0;

	return NULL;
}


//Messages that need to be sent periodically
/*void *btServer_sendMessageHandler(void * arg)
{
	const int DIA_PERIOD = 500;
	const int TRQ_PERIOD = 1500;
	const int SLEEP_TIME = 250;
	
	int diatime = 0;
	int trqtime = 0;
    
    VehicleState previousState;
	qwarnGetCurrentVehicleState(&previousState);
	
	while(1)
	{
	    VehicleState currentState;
		qwarnGetCurrentVehicleState(&currentState);
		
		if(strcmp(previousState.roadwayId, currentState.roadwayId) != 0){
			sendClrQWarnUIAlert();
			sendClrSpdHarmUIAlert();
			previousState = currentState;
		}
		diatime += SLEEP_TIME;
		trqtime += SLEEP_TIME;

		if (diatime > DIA_PERIOD)
		{
			diatime -= DIA_PERIOD;
			sendDIA();
		}

		if (trqtime > TRQ_PERIOD)
		{
			trqtime -= TRQ_PERIOD;
			
			if (!RSU_Flag)
				sendTRQ();
		}
		usleep(SLEEP_TIME * 1000);
		pthread_testcancel();
	}
}*/

void btServer_destroyClients()
{
	if (btServerSocket != 0)
	{
		//close socket
		int returnVal = close(btServerSocket);
		if (returnVal < 0)
		{
			DBG_ERR(DBGM_BT, printf("BtServer: Error Closing Server Socket ...\n"));
		}
		else
		{
			DBG_INFO(DBGM_BT, printf("BtServer: Bluetooth Server Socket Closed\n"));
		}

		btServerSocket = 0;
	}

	btServer_closeSdp();

	int i;
	for(i=0; i<MAX_CONNECTIONS; i++)
	{
		if(btClients[i].sock != (int) NULL)	//If it is not null then it exist so try to cancel it
		{
			btClients[i].state = DISCONNECTED;
			shutdown(btClients[i].sock, SHUT_RD);
			if(!(pthread_join(btClients[i].threadId,NULL)))
			{
				DBG(DBGM_BT, printf("BtServer: Bluetooth Client Thread (%d) Joined\n", i));
			}
		}
	}
}


int btServer_str2uuid(const char *uuid_str, uuid_t *uuid)
{
    uint32_t uuid_int[4];
    char *endptr;

    if( strlen( uuid_str ) == 36 ) {
        // Parse uuid128 standard format: 12345678-9012-3456-7890-123456789012
        char buf[9] = { 0 };

        if( uuid_str[8] != '-' && uuid_str[13] != '-' &&
            uuid_str[18] != '-'  && uuid_str[23] != '-' ) {
            return 0;
        }
        // first 8-bytes
        strncpy(buf, uuid_str, 8);
        uuid_int[0] = htonl( strtoul( buf, &endptr, 16 ) );
        if( endptr != buf + 8 ) return 0;

        // second 8-bytes
        strncpy(buf, uuid_str+9, 4);
        strncpy(buf+4, uuid_str+14, 4);
        uuid_int[1] = htonl( strtoul( buf, &endptr, 16 ) );
        if( endptr != buf + 8 ) return 0;

        // third 8-bytes
        strncpy(buf, uuid_str+19, 4);
        strncpy(buf+4, uuid_str+24, 4);
        uuid_int[2] = htonl( strtoul( buf, &endptr, 16 ) );
        if( endptr != buf + 8 ) return 0;

        // fourth 8-bytes
        strncpy(buf, uuid_str+28, 8);
        uuid_int[3] = htonl( strtoul( buf, &endptr, 16 ) );
        if( endptr != buf + 8 ) return 0;

        if( uuid != NULL ) sdp_uuid128_create( uuid, uuid_int );
    } else if ( strlen( uuid_str ) == 8 ) {
        // 32-bit reserved UUID
        uint32_t i = strtoul( uuid_str, &endptr, 16 );
        if( endptr != uuid_str + 8 ) return 0;
        if( uuid != NULL ) sdp_uuid32_create( uuid, i );
    } else if( strlen( uuid_str ) == 4 ) {
        // 16-bit reserved UUID
        int i = strtol( uuid_str, &endptr, 16 );
        if( endptr != uuid_str + 4 ) return 0;
        if( uuid != NULL ) sdp_uuid16_create( uuid, i );
    } else {
        return 0;
    }

    return 1;
}
