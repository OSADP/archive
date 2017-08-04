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

#include <pthread.h>

// Definitions
typedef enum { DISCONNECTED, CONNECTED } ConnectionState;

#define MAX_CONNECTIONS	1
#define BT_MSG_LENGTH 	3500
#define BT_MAC_STRING_LENGTH 50

typedef struct
{
	ConnectionState state;
	int sock;
	pthread_t threadId;
	char macString[BT_MAC_STRING_LENGTH];

} BtClient;

void btServer_start();
void btServer_stop();
void btServer_setRxMessageCallback(void (*callback)(char *));
void btServer_sendMessageToAllClients(char *message);

#endif
