#ifndef	_BTServer_H_
#define	_BTServer_H_

#include "bluetooth/bluetooth.h"
#include "bluetooth/hci.h"
#include "bluetooth/hci_lib.h"
#include "bluetooth/sdp.h"
#include "bluetooth/sdp_lib.h"
#include "bluetooth/rfcomm.h"
#include "TravelerInformation.h"
#include "wave.h"

// Definitions
#define DISCONNECTED	0
#define CONNECTED		1
#define MAX_CONNECTIONS	10
#define RSUTIMEOUT 3

typedef struct
{
	int status;
	int client_sock;
	pthread_t thread_id;

} net_connection_t;

void*	bt_server( void *data );
void*	btConnectionHandler(net_connection_t *net_connection);
void 	sendMessageToAllClients(char *message);
void*	sendConnectionHandler(void *arg);
void 	btDestroy();

#endif
