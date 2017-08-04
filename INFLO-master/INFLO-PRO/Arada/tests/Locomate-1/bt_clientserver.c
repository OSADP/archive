#include <stdio.h>
#include <pthread.h>
#include <unistd.h>
#include <sys/socket.h>
#include "bluetooth/bluetooth.h"
#include "bluetooth/rfcomm.h"

extern void server_thread(void);
extern void client_thread(void);

static pthread_t server_thread_id;
static pthread_t client_thread_id;

/**
 * This will set the adapter to a known state using linux commands
 * Initialize the BT adapter
 * 		Take the adapter down to clear all connections
 * 		Bring the adapter up
 * 		Give it a name
 * 		Make it discoverable
 */
void initializeAdapter() {
	// Reset Bluetooth and Initialize
	char cmd[50];
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 down\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 up\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 name 'Arada1'\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 piscan\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
}

int main(int argc, char **argv)
{

	server_thread_id = 0;
	client_thread_id = 0;
	pthread_create(&server_thread_id, 0, server_thread, 0);
	pthread_create(&client_thread_id, 0, client_thread, 0);
	pthread_join(server_thread_id,NULL);
	pthread_join(client_thread_id,NULL);
	return 0;
}
