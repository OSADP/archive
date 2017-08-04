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
#include <assert.h>
#include <errno.h>
#include <fcntl.h>

#include "Debug.h"

void btServer_initAdapter();
void btServer_registerSdp(BtServer *server);
void btServer_closeSdp(BtServer *server);
void *btServer_serverStart(void *arg);
void btServer_handleMessage(char *msg);
void *btServer_clientRun(void *arg);
void btServer_destroyClients();
int btServer_str2uuid(const char *uuid_str, uuid_t *uuid);

static int initialized = 0;
static pthread_mutex_t initLock = PTHREAD_MUTEX_INITIALIZER;


/***********************************************
 * EXPOSED METHODS
 **********************************************/

BtServer *btServer_create()//DONE
{
	BtServer *results = (BtServer *)calloc(1, sizeof(BtServer));
	if (!results)
		return NULL;

	results->sdpSession = 0;

	if (!(results->thread = thread_create()))
	{
		btServer_destroy(results);
		return NULL;
	}

	if (!(results->txLock = lock_create()))
	{
		btServer_destroy(results);
		return NULL;
	}

	results->btServerSocket = 0;

	int i;
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		results->btConnections[i].sock = (int) NULL;
		results->btConnections[i].state = DISCONNECTED;
		results->btConnections[i].thread = thread_create();
		results->btConnections[i].server = results;
	}
	
	results->rxMsgCallback = NULL;

	return results;
}


void btServer_destroy(BtServer *server)//DONE
{
	assert(server != NULL);

	btServer_stop(server);

	if (server->thread)
	{
		btServer_stop(server);
		thread_destroy(server->thread);
	}

	if (server->txLock)
		lock_destroy(server->txLock);

	free(server);
}


void btServer_start(BtServer *server, char *uuid, int rfcommChan)//DONE
{
	assert(server != NULL);

	strncpy(server->uuid, uuid, BT_UUID_STRING_LENGTH);
	server->rfcommChan = rfcommChan;

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Starting on RFCOMM Channel %d\n", server->uuid, server->rfcommChan));

	btServer_registerSdp(server);
	thread_start(server->thread, btServer_serverStart, server);
}

void btServer_stop(BtServer *server)//DONE
{
	assert(server != NULL);

	if (!thread_isRunning(server->thread))
		return;

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Stopping\n", server->uuid));
	shutdown(server->btServerSocket, SHUT_RDWR);
	//thread_cancel(server->thread);
	thread_join(server->thread);

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Server stopped, closing SDP Connection\n", server->uuid));
	btServer_closeSdp(server);

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): SDP Connection closed, destroying clients\n", server->uuid));
	btServer_destroyClients(server);
}

void btServer_setRxMessageCallback(BtServer *server, void (*rxMsgCallback)(void *, char *, int), void *userPntr)
{
	assert(server != NULL);
	server->userPntr = userPntr;
	server->rxMsgCallback = rxMsgCallback;
}

void btServer_sendMessageToAllClients(BtServer *server, char *message, int length)//DONE
{
	assert(server != NULL);
	assert(message != NULL);

	if (!thread_isRunning(server->thread))
		return;

	lock_lock(server->txLock);
	
	int i;
	for(i = 0; i < MAX_CONNECTIONS; i++)
	{
		if (server->btConnections[i].state == CONNECTED) {
			int retvalue = write(server->btConnections[i].sock, message, length);
			if (retvalue < 0)
			{
				DBG_ERR(DBGM_BT, printf("Bluetooth Server (%s): Error Sending to Bluetooth Connection %d. Setting to DISCONNECTED\n", server->uuid, i));
				server->btConnections[i].state = DISCONNECTED;
			}
		}
	}

	lock_unlock(server->txLock);
}


/***********************************************
 * INTERNAL METHODS
 **********************************************/

/*
 * Reference: "Bluetooth for Programmers". 2005. Huang, Albert & Rudolph, Larry. http://people.csail.mit.edu/rudolph/Teaching/Articles/BTBook.pdf
 */
void btServer_registerSdp(BtServer *server)//DONE
{
	assert(server != NULL);

	if (server->sdpSession)
		return;
		
	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Registering with UUID: %s ...\n", server->uuid, server->uuid));

	sdp_record_t *rec = sdp_record_alloc();
	uuid_t l2cap_uuid, root_uuid, rfcomm_uuid, service_uuid;
	sdp_list_t *service_class_list = 0, *l2cap_list = 0, *rfcomm_list = 0, *root_list = 0, *protocol_list = 0, *access_protocol_list = 0;
	sdp_data_t *channel = 0;
	uint8_t rfcomm_chan = server->rfcommChan;

	if(!btServer_str2uuid(server->uuid, &service_uuid))
	{
		perror("Bluetooth Server: Invalid UUID");
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

	server->sdpSession = sdp_connect(BDADDR_ANY, BDADDR_LOCAL, SDP_RETRY_IF_BUSY);
	if (server->sdpSession != NULL)
	{
		sdp_record_register(server->sdpSession, rec, 0);

		DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): SDP Record Registered\n", server->uuid));
	}
	else
	{
		DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Unable to connect to SDP Server.\n", server->uuid));
	}

	//Free service lists
	sdp_data_free(channel);
	sdp_list_free(l2cap_list, 0);
	sdp_list_free(rfcomm_list, 0);
	sdp_list_free(root_list, 0);
	sdp_list_free(access_protocol_list, 0);
}

void btServer_closeSdp(BtServer *server)//DONE
{
	if (!server->sdpSession)
		return;

	sdp_close(server->sdpSession);
	server->sdpSession = 0;

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Closed connection to SDP Service\n", server->uuid));
}

void btServer_initAdapter()//DONE
{

	pthread_mutex_lock(&initLock);

	if (!initialized)
	{
		initialized = 1;
		// Reset Bluetooth and Initialize
		char cmd[50];

		sprintf(cmd, "/usr/local/bin/hciconfig hci0 up");
		DBG_INFO(DBGM_BT, printf("BtServer: Executing command:%s\n", cmd));
		system(cmd);

		sprintf(cmd, "/usr/local/bin/hciconfig hci0 piscan");
		DBG_INFO(DBGM_BT, printf("BtServer: Executing command:%s\n", cmd));
		system(cmd);
	}

	pthread_mutex_unlock(&initLock);
}

void *btServer_serverStart(void *args)//DON??
{
	BtServer *server = args;
	assert(server != NULL);

	btServer_initAdapter();
	btServer_destroyClients(server);

	// allocate socket
	DBG(DBGM_BT, printf("Bluetooth Server (%s): Allocating socket...\n", server->uuid));
	server->btServerSocket = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

	struct sockaddr_rc loc_addr = { 0 };
	loc_addr.rc_family = AF_BLUETOOTH;
	loc_addr.rc_bdaddr = *BDADDR_ANY;
	loc_addr.rc_channel = server->rfcommChan;

	//Bind to socket
	DBG(DBGM_BT, printf("Bluetooth Server (%s): Binding socket...\n", server->uuid));
	if(bind(server->btServerSocket, (struct sockaddr *)&loc_addr, sizeof(loc_addr)))
		perror("BIND");

	// put socket into listening mode
	DBG(DBGM_BT, printf("Bluetooth Server (%s): Listening...\n", server->uuid));
	listen(server->btServerSocket, 1);

	int flags;

    if (-1 == (flags = fcntl(server->btServerSocket, F_GETFL, 0)))
        flags = 0;
    fcntl(server->btServerSocket, F_SETFL, flags | O_NONBLOCK);

	DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Started\n", server->uuid));

	while(thread_isRunning(server->thread))
	{
		struct sockaddr_rc rem_addr = { 0 };
		socklen_t opt = sizeof(rem_addr);
		int client = accept(server->btServerSocket, (struct sockaddr *)&rem_addr, &opt);

		if (client <= 0 && (errno == EAGAIN || errno == EWOULDBLOCK))
		{
			usleep(10000);
			continue;
		}
		
		int i;
		for(i = 0; (i < MAX_CONNECTIONS) & (server->btConnections[i].sock != (int) NULL); i++);	//look for first available socket fd

		if(i < MAX_CONNECTIONS)
		{
			if(client > 0)
			{
				if (thread_isRunning(server->btConnections[i].thread))
					thread_join(server->btConnections[i].thread);
			
				char buf[30];
				ba2str( &rem_addr.rc_bdaddr, buf );
				DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Accepted connection #%d from %s\n", server->uuid, i, buf));

				strncpy(server->btConnections[i].macString, buf, BT_MAC_STRING_LENGTH);
				server->btConnections[i].sock = client;
				server->btConnections[i].state = CONNECTED;
				thread_start(server->btConnections[i].thread, btServer_clientRun, (void *)&server->btConnections[i]);
			}

		}
		else
		{
			if(client > 0)
				close(client);

			DBG_ERR(DBGM_BT, printf("Bluetooth Server (%s): Unable to accept new connection.  Max number of connections reached.\n", server->uuid));
		}

		usleep(250000);
	}

	if (server->btServerSocket)
	{
		//close socket
		int returnVal = close(server->btServerSocket);
		if (returnVal < 0)
		{
			DBG_ERR(DBGM_BT, printf("Bluetooth Server (%s): Error Closing Server Socket\n", server->uuid));
		}
		else
		{
			DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Bluetooth Server Socket Closed\n", server->uuid));
		}
		server->btServerSocket = 0;
	}

	return 0;
}

void *btServer_clientRun(void *arg)
{
	BtConnection *client = (BtConnection *)arg;
	assert(client != NULL);

	char ch[1000];

	while(client->state == CONNECTED)
	{
		int retvalue = read(client->sock, &ch, sizeof(ch));
		if(retvalue <= 0)	//connection has been closed
		{
			DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s) - BtConnection '%s': Bluetooth Server Socket Closed\n", client->server->uuid, client->macString));
			client->state = DISCONNECTED;
		} 
		else if(retvalue > 0)
		{	
			if (client->server->rxMsgCallback)
				client->server->rxMsgCallback(client->server->userPntr, ch, retvalue);
		}
	}

	int retvalue = close(client->sock);
	if (retvalue < 0)
	{
		DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s) - BtConnection '%s': Error Closing Socket...\n", client->server->uuid, client->macString));
	}
	else
	{
		DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s) - BtConnection '%s': Bluetooth Socket Closed\n", client->server->uuid, client->macString));	
	}
	client->macString[0] = '\0';
	client->sock = 0;

	return NULL;
}

void btServer_destroyClients(BtServer *server)
{
	assert(server != NULL);

	int i;
	for(i = 0; i < MAX_CONNECTIONS; i++)
	{
		if(server->btConnections[i].sock != (int) NULL)	//If it is not null then it exist so try to cancel it
		{
			server->btConnections[i].state = DISCONNECTED;
			shutdown(server->btConnections[i].sock, SHUT_RD);
			thread_join(server->btConnections[i].thread);
			DBG_INFO(DBGM_BT, printf("Bluetooth Server (%s): Bluetooth Client Thread (%d) Joined\n", server->uuid, i));
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
