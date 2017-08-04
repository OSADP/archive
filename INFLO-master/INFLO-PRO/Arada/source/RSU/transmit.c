#include <pthread.h>
#include <stdio.h>


#include "infloRSE.h"
#include "network.h"

static pthread_t netServer_thread_id;

int main( int argc, char *argv[] )
{

    printf("Battelle RSU DSRC Implementation\n");
    netServer_thread_id = (int) NULL;
    pthread_create(&netServer_thread_id, NULL, (void*) netServer, 0);
    printf("Created GUI Server\n");

    startRSU(argc, argv);
	// Start local network for commands


	pthread_join( netServer_thread_id, NULL );
    printf("Battelle RSU DSRC Closing\n");
}


