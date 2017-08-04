#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include "bluetooth/bluetooth.h"
#include "bluetooth/rfcomm.h"

void client_thread(void)
{
    struct sockaddr_rc addr = { 0 };
    int s, status;
    char buf[1024] = { 0 };
    int bytes_read;

   //char dest[18] = "00:1F:81:00:08:30";
   //char dest[18] = "00:10:60:57:12:F7";
   char dest[18] = "00:10:60:57:16:12";
   while(1) {
   // allocate a socket
    s = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

    // set the connection parameters (who to connect to)
    addr.rc_family = AF_BLUETOOTH;
    addr.rc_channel = (uint8_t) 1;
    str2ba( dest, &addr.rc_bdaddr );

    printf("Connecting to BT\n");


    	// connect to server
    	status = connect(s, (struct sockaddr *)&addr, sizeof(addr));

    	printf("Status %d\n", status);

    	// send a message
    	if( status == 0 ) {
    		printf("Sending get time\n");
    		status = write(s, "%GET TIME;\r\n", 11);
    	}


    	if( status < 0 )
    	{
    		perror("Error");
    	}
    	else
    	{
    		printf("Reading...\n");

    		// read data from the client
    		bytes_read = read(s, buf, sizeof(buf));
    		if( bytes_read > 0 ) {
    			printf("received [%s]\n", buf);
    		}
    		else
    		{
    			perror("Error");
    		}
    	}
    	sleep(2);
    }
  //  close(s);

}
