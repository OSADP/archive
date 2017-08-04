/**
 * @file         BtServer.h
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


#ifndef	_BTSERVER_H_
#define	_BTSERVER_H_

#include "Thread.h"
#include "Lock.h"

#include <pthread.h>
#include <bluetooth/bluetooth.h>
#include <bluetooth/hci.h>
#include <bluetooth/hci_lib.h>
#include <bluetooth/sdp.h>
#include <bluetooth/sdp_lib.h>
#include <bluetooth/rfcomm.h>

// Definitions
typedef enum { DISCONNECTED, CONNECTED } ConnectionState;

#define MAX_CONNECTIONS	5
#define BT_MAC_STRING_LENGTH 50
#define BT_UUID_STRING_LENGTH 50

struct BtServer;

typedef struct
{
	volatile ConnectionState state;
	int sock;
	Thread *thread;
	char macString[BT_MAC_STRING_LENGTH];
	struct BtServer *server;

} BtConnection;

typedef struct BtServer
{
	sdp_session_t *sdpSession;
	Thread *thread;
	Lock *txLock;
	char uuid[BT_UUID_STRING_LENGTH];
	int rfcommChan;
	int btServerSocket;
	BtConnection btConnections[MAX_CONNECTIONS];
	void *userPntr;
	void (*rxMsgCallback)(void *, char *, int);
} BtServer;


BtServer *btServer_create();
void btServer_destroy(BtServer *server);

void btServer_start(BtServer *server, char *uuid, int rfcommChan);
void btServer_stop(BtServer *server);
void btServer_setRxMessageCallback(BtServer *server, void (*rxMsgCallback)(void *, char *, int), void *userPntr);
void btServer_sendMessageToAllClients(BtServer *server, char *message, int length);

#endif
